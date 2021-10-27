using System;
using System.Collections.Generic;

namespace BlazorDataTable.Client.DataTable
{
    public class DataTableModel<T>
    {
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public List<T> Items { get; set; }
        public List<DataTableHeaderModel> Headers { get; set; }

        public DataTableModel(List<T> items, int count, int pageIndex, int pageSize, List<DataTableHeaderModel> headers)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            Items = new List<T>();
            Items.AddRange(items);

            Headers = headers;
        }

        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 1);
            }
            set { }
        }

        public bool HasNextPage
        {
            get
            {
                return (PageIndex < TotalPages);
            }
            set { }
        }
    }

    public class DataTableHeaderModel
    {
        public string Header { get; set; }
        public string SortBy { get; set; }
        public int OrderBy { get; set; }
    }
}
