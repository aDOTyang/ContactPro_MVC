using ContactPro_MVC.Models;

namespace ContactPro_MVC.Services.Interfaces
{
    public interface IAddressBookService
    {
        // without a type signifier, Task returns nothing (void)
        public Task AddContactToCategoryAsync(int categoryId, int contactId);
        // one contact, but allows for multiple categories - pluralizes the above task
        public Task AddContactToCategoriesAsync(IEnumerable<int> categoryIds, int contactId);
        public Task<bool> IsContactInCategory(int categoryId, int contactId);
        public Task<IEnumerable<Category>> GetAppUserCategoriesAsync(string appUserId);
        // removes contact from all categories
        public Task RemoveAllContactCategoriesAsync(int contactId);
    }
}
