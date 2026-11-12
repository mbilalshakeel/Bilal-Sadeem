using CRMwithAD.Module.BusinessObjects;
using DevExpress.Xpo;
using Xunit;

namespace CRMwithAD.E2E.Tests
{
    public class ApplicationUserTests
    {
        [Fact]
        public void CanCreateUser_WithValidData()
        {
            // Arrange
            using var uow = new UnitOfWork();
            var user = new ApplicationUser(uow)
            {
                Name = "Test User",
                Email = "test@example.com",
                UserName = "testuser"
            };

            // Act
            uow.CommitChanges();

            // Assert
            Assert.NotNull(user.Oid);
            Assert.Equal("Test User", user.Name);
        }

        [Fact]
        public void CannotCreateUser_WithoutUserName()
        {
            using var uow = new UnitOfWork();
            var user = new ApplicationUser(uow)
            {
                Name = "Test User",
                Email = "test@example.com"
                // UserName missing
            };

            Assert.Throws<ArgumentNullException>(() => uow.CommitChanges());
        }
    }
}
