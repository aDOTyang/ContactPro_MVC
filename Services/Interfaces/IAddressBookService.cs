namespace ContactPro_MVC.Services.Interfaces
{
    public interface IAddressBookService
    {
        // without a type signifier, Task returns nothing (void)
        public Task AddContactToCategoryAsync(int categoryId, int contactId);
        Task<bool> IsContactInCategory(int categoryId, int contactId);
    }
}
