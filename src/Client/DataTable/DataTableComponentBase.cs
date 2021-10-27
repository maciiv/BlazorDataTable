using BlazorDataTable.Client.DataTable;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace BlazorDataTable.Client.Shared
{
    public class DataTableComponentBase<T> : ComponentBase
    {
        public DataTableModel<T> Create(List<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count;
            var items = source.Distinct().Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new DataTableModel<T>(items, count, pageIndex, pageSize, GetHeaders());
        }

        public async Task<List<T>> Sort(List<T> source, string sortField, string sortOrder)
        {
            var queryElementTypeParam = Expression.Parameter(typeof(T));
            var memberAccess = Expression.PropertyOrField(queryElementTypeParam, sortField);
            var keySelector = Expression.Lambda(memberAccess, queryElementTypeParam);

            var orderBy = Expression.Call(
                typeof(Queryable),
                sortOrder.ToLower() == "asc" ? "OrderBy" : "OrderByDescending",
                new Type[] { typeof(T), memberAccess.Type },
                source.AsQueryable().Expression,
                Expression.Quote(keySelector));

            return await Task.Run(() => source.AsQueryable().Provider.CreateQuery<T>(orderBy).ToList());
        }

        public async Task<List<T>> Search(List<T> source, string searchTerm)
        {
            var result = new List<T>();
            var props = typeof(T).GetProperties();

            var parameter = Expression.Parameter(typeof(T));
            var target = Expression.Constant(searchTerm.ToLower(), typeof(string));
            var contains = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
            var trim = typeof(string).GetMethod("Trim", Type.EmptyTypes);
            var lower = typeof(string).GetMethod("ToLower", Type.EmptyTypes);

            foreach (var prop in props)
            {
                var member = Expression.PropertyOrField(parameter, prop.Name);
                var toString = Expression.Call(Expression.Convert(member, typeof(object)), typeof(object).GetMethod("ToString"));
                var notNullSource = await RemoveNulls(source, parameter, toString);
                if (!notNullSource.Any()) continue;
              
                var propTrim = Expression.Call(toString, trim);
                var propLower = Expression.Call(propTrim, lower);
                var method = Expression.Call(propLower, contains, target);

                var query = Expression.Call(
                    typeof(Queryable),
                    "Where",
                    new Type[] { notNullSource.AsQueryable().ElementType },
                    notNullSource.AsQueryable().Expression,
                    Expression.Lambda<Func<T, bool>>(method, parameter));
                var search = await Task.Run(() => notNullSource.AsQueryable().Provider.CreateQuery<T>(query).ToList());

                result.AddRange(search);
            }

            return result.Distinct().ToList();
        }

        private List<DataTableHeaderModel> GetHeaders()
        {
            var result = new List<DataTableHeaderModel>();
            var props = typeof(T).GetProperties();

            foreach (var prop in props)
            {
                var customAttribute = prop.GetCustomAttribute<DisplayAttribute>();
                if (customAttribute == null) continue;
                var header = customAttribute != null ? customAttribute.Name : prop.Name;

                result.Add(new DataTableHeaderModel
                {
                    Header = customAttribute.Name,
                    SortBy = prop.Name,
                    OrderBy = customAttribute.Order
                });
            }

            return result.OrderBy(c => c.OrderBy).ToList();
        }

        private async Task<List<T>> RemoveNulls(List<T> source, ParameterExpression parameter, MethodCallExpression toString)
        {
            var notNull = Expression.NotEqual(toString, Expression.Constant(null));
            var removeNull = Expression.Call(
                typeof(Queryable),
                "Where",
                new Type[] { source.AsQueryable().ElementType },
                source.AsQueryable().Expression,
                Expression.Lambda<Func<T, bool>>(notNull, parameter));
            return await Task.Run(() => source.AsQueryable().Provider.CreateQuery<T>(removeNull).ToList());
        }
    }
}
