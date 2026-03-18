using client_contact_management.Controllers;
using client_contact_management.Models;
using client_contact_management.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace client_contact_management.tests
{
    public class ClientControllerTests
    {
        private readonly Mock<IClientService> _clientServiceMock;
        private readonly Mock<IContactService> _contactServiceMock;
        private readonly ClientController _controller;
        private readonly CancellationToken _ct = CancellationToken.None;
        public ClientControllerTests()
        {
            _clientServiceMock = new Mock<IClientService>();
            _contactServiceMock = new Mock<IContactService>();

            _controller = new ClientController(
                _clientServiceMock.Object,
                _contactServiceMock.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        private void SetAjaxRequest()
        {
            _controller.HttpContext.Request.Headers["X-Requested-With"] = "XMLHttpRequest";
        }

        private static ClientResponse MakeClientResponse(int id = 1) => new ClientResponse
        {
            Id = id,
            Name = "Acme Corp",
            ClientCode = "ACM",
            LinkedContacts = new List<ContactResponse>()
        };

        [Fact]
        public async Task Index_ReturnsViewWithClients()
        {
            var clients = new List<ClientResponse> { MakeClientResponse() };
            _clientServiceMock.Setup(s => s.GetAllAsync(_ct)).ReturnsAsync(clients);

            var result = await _controller.Index(_ct);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(clients, viewResult.Model);
        }

        [Fact]
        public void Create_Get_ReturnsViewWithEmptyRequest()
        {
            var result = _controller.Create();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ClientRequest>(viewResult.Model);
        }

        [Fact]
        public async Task Create_Post_ValidModel_RedirectsToEdit()
        {
            var request = new ClientRequest { Name = "Acme", ClientCode = "ACM" };
            _clientServiceMock.Setup(s => s.AddAsync(request, _ct)).ReturnsAsync(42);

            var result = await _controller.Create(request, _ct);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirect.ActionName);
            Assert.Equal(42, redirect.RouteValues!["id"]);
        }

        [Fact]
        public async Task Create_Post_ValidModel_Ajax_ReturnsJsonWithId()
        {
            SetAjaxRequest();
            var request = new ClientRequest { Name = "Acme", ClientCode = "ACM" };
            _clientServiceMock.Setup(s => s.AddAsync(request, _ct)).ReturnsAsync(42);

            var result = await _controller.Create(request, _ct);

            var json = Assert.IsType<JsonResult>(result);
            // anonymous type — check via reflection
            var id = json.Value!.GetType().GetProperty("id")!.GetValue(json.Value);
            Assert.Equal(42, id);
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsView()
        {
            _controller.ModelState.AddModelError("Name", "Required");
            var request = new ClientRequest();

            var result = await _controller.Create(request, _ct);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(request, viewResult.Model);
        }

        [Fact]
        public async Task Create_Post_InvalidModel_Ajax_ReturnsBadRequest()
        {
            SetAjaxRequest();
            _controller.ModelState.AddModelError("Name", "Required");

            var result = await _controller.Create(new ClientRequest(), _ct);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Edit_Get_KnownId_ReturnsViewWithMappedRequest()
        {
            var response = MakeClientResponse(7);
            _clientServiceMock.Setup(s => s.GetByIdAsync(7, _ct)).ReturnsAsync(response);
            _contactServiceMock.Setup(s => s.GetAllAsync(_ct)).ReturnsAsync(new List<ContactResponse>());

            var result = await _controller.Edit(7, _ct);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ClientRequest>(viewResult.Model);
            Assert.Equal(7, model.Id);
            Assert.Equal("Acme Corp", model.Name);
            Assert.Equal("ACM", model.ClientCode);
        }

        [Fact]
        public async Task Edit_Get_UnknownId_ReturnsNotFound()
        {
            _clientServiceMock.Setup(s => s.GetByIdAsync(99, _ct)).ReturnsAsync((ClientResponse?)null);

            var result = await _controller.Edit(99, _ct);

            Assert.IsType<NotFoundResult>(result);
        }


        [Fact]
        public async Task Edit_Post_ValidModel_RedirectsToEdit()
        {
            var request = new ClientRequest { Id = 5, Name = "Updated", ClientCode = "UPD" };

            var result = await _controller.Edit(request, _ct);

            _clientServiceMock.Verify(s => s.UpdateAsync(request, _ct), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirect.ActionName);
            Assert.Equal(5, redirect.RouteValues!["id"]);
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsView()
        {
            _controller.ModelState.AddModelError("Name", "Required");
            var request = new ClientRequest { Id = 5 };
            var response = MakeClientResponse(5);
            _clientServiceMock.Setup(s => s.GetByIdAsync(5, _ct)).ReturnsAsync(response);
            _contactServiceMock.Setup(s => s.GetAllAsync(_ct)).ReturnsAsync(new List<ContactResponse>());

            var result = await _controller.Edit(request, _ct);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(request, viewResult.Model);
        }

        [Fact]
        public async Task LinkContact_NonAjax_RedirectsToEdit()
        {
            var result = await _controller.LinkContact(1, 2, _ct);

            _clientServiceMock.Verify(s => s.LinkContactAsync(1, 2, _ct), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirect.ActionName);
        }

        [Fact]
        public async Task LinkContact_Ajax_ReturnsOk()
        {
            SetAjaxRequest();

            var result = await _controller.LinkContact(1, 2, _ct);

            Assert.IsType<OkResult>(result);
        }

 

        [Fact]
        public async Task UnlinkContact_NonAjax_RedirectsToEdit()
        {
            var result = await _controller.UnlinkContact(1, 2, _ct);

            _clientServiceMock.Verify(s => s.UnlinkContactAsync(1, 2, _ct), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirect.ActionName);
        }

        [Fact]
        public async Task UnlinkContact_Ajax_ReturnsOk()
        {
            SetAjaxRequest();

            var result = await _controller.UnlinkContact(1, 2, _ct);

            Assert.IsType<OkResult>(result);
        }


        [Fact]
        public async Task Delete_Get_KnownId_ReturnsView()
        {
            var response = MakeClientResponse(3);
            _clientServiceMock.Setup(s => s.GetByIdAsync(3, _ct)).ReturnsAsync(response);

            var result = await _controller.Delete(3, _ct);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(response, viewResult.Model);
        }

        [Fact]
        public async Task Delete_Get_UnknownId_ReturnsNotFound()
        {
            _clientServiceMock.Setup(s => s.GetByIdAsync(99, _ct)).ReturnsAsync((ClientResponse?)null);

            var result = await _controller.Delete(99, _ct);

            Assert.IsType<NotFoundResult>(result);
        }


        [Fact]
        public async Task DeleteConfirmed_NonAjax_DeletesAndRedirectsToIndex()
        {
            var result = await _controller.DeleteConfirmed(3, _ct);

            _clientServiceMock.Verify(s => s.DeleteAsync(3, _ct), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task DeleteConfirmed_Ajax_DeletesAndReturnsOk()
        {
            SetAjaxRequest();

            var result = await _controller.DeleteConfirmed(3, _ct);

            _clientServiceMock.Verify(s => s.DeleteAsync(3, _ct), Times.Once);
            Assert.IsType<OkResult>(result);
        }


    }
}
