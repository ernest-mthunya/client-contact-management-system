using client_contact_management.Controllers;
using client_contact_management.Models;
using client_contact_management.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace client_contact_management.tests
{
    public class ContactControllerTests
    {
        private readonly Mock<IContactService> _contactServiceMock;
        private readonly Mock<IClientService> _clientServiceMock;
        private readonly ContactController _controller;
        private readonly CancellationToken _ct = CancellationToken.None;

        public ContactControllerTests()
        {
            _contactServiceMock = new Mock<IContactService>();
            _clientServiceMock = new Mock<IClientService>();

            _controller = new ContactController(
                _contactServiceMock.Object,
                _clientServiceMock.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        private void SetAjaxRequest() =>
           _controller.HttpContext.Request.Headers["X-Requested-With"] = "XMLHttpRequest";

        private static ContactResponse MakeContactResponse(int id = 1) => new ContactResponse
        {
            Id = id,
            Name = "Jane",
            Surname = "Doe",
            Email = "jane.doe@example.com",
            LinkedClients = new List<ClientResponse>()
        };

        private static ContactRequest MakeContactRequest(int id = 0) => new ContactRequest
        {
            Id = id,
            Name = "Jane",
            Surname = "Doe",
            Email = "jane.doe@example.com"
        };

        [Fact]
        public async Task Index_ReturnsViewWithAllContacts()
        {
            var contacts = new List<ContactResponse> { MakeContactResponse() };
            _contactServiceMock.Setup(s => s.GetAllAsync(_ct)).ReturnsAsync(contacts);

            var result = await _controller.Index(_ct);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(contacts, viewResult.Model);
        }

        [Fact]
        public void Create_Get_ReturnsViewWithEmptyRequest()
        {
            var result = _controller.Create();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ContactRequest>(viewResult.Model);
        }


        [Fact]
        public async Task Create_Post_ValidModel_UniqueEmail_RedirectsToEdit()
        {
            var request = MakeContactRequest();
            _contactServiceMock.Setup(s => s.IsEmailUniqueAsync(request.Email, null, _ct)).ReturnsAsync(true);
            _contactServiceMock.Setup(s => s.AddAsync(request, _ct)).ReturnsAsync(10);

            var result = await _controller.Create(request, _ct);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirect.ActionName);
            Assert.Equal(10, redirect.RouteValues!["id"]);
        }

        [Fact]
        public async Task Create_Post_ValidModel_UniqueEmail_Ajax_ReturnsJsonWithId()
        {
            SetAjaxRequest();
            var request = MakeContactRequest();
            _contactServiceMock.Setup(s => s.IsEmailUniqueAsync(request.Email, null, _ct)).ReturnsAsync(true);
            _contactServiceMock.Setup(s => s.AddAsync(request, _ct)).ReturnsAsync(10);

            var result = await _controller.Create(request, _ct);

            var json = Assert.IsType<JsonResult>(result);
            var id = json.Value!.GetType().GetProperty("id")!.GetValue(json.Value);
            Assert.Equal(10, id);
        }

        [Fact]
        public async Task Create_Post_DuplicateEmail_ReturnsView()
        {
            var request = MakeContactRequest();
            _contactServiceMock.Setup(s => s.IsEmailUniqueAsync(request.Email, null, _ct)).ReturnsAsync(false);

            var result = await _controller.Create(request, _ct);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(request, viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Create_Post_DuplicateEmail_Ajax_ReturnsBadRequestWithError()
        {
            SetAjaxRequest();
            var request = MakeContactRequest();
            _contactServiceMock.Setup(s => s.IsEmailUniqueAsync(request.Email, null, _ct)).ReturnsAsync(false);

            var result = await _controller.Create(request, _ct);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var error = badRequest.Value!.GetType().GetProperty("error")!.GetValue(badRequest.Value) as string;
            Assert.NotNull(error);
            Assert.Contains("already in use", error);
        }


        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsView()
        {
            _controller.ModelState.AddModelError("Name", "Required");
            var request = new ContactRequest { Email = "a@b.com" };
            _contactServiceMock.Setup(s => s.IsEmailUniqueAsync(request.Email, null, _ct)).ReturnsAsync(true);

            var result = await _controller.Create(request, _ct);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(request, viewResult.Model);
        }

        [Fact]
        public async Task Create_Post_InvalidModel_Ajax_ReturnsBadRequest()
        {
            SetAjaxRequest();
            _controller.ModelState.AddModelError("Name", "Required");
            var request = new ContactRequest { Email = "a@b.com" };
            _contactServiceMock.Setup(s => s.IsEmailUniqueAsync(request.Email, null, _ct)).ReturnsAsync(true);

            var result = await _controller.Create(request, _ct);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Edit_Get_KnownId_ReturnsViewWithMappedRequest()
        {
            var response = MakeContactResponse(5);
            _contactServiceMock.Setup(s => s.GetByIdAsync(5, _ct)).ReturnsAsync(response);
            _clientServiceMock.Setup(s => s.GetAllAsync(_ct)).ReturnsAsync(new List<ClientResponse>());

            var result = await _controller.Edit(5, _ct);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ContactRequest>(viewResult.Model);
            Assert.Equal(5, model.Id);
            Assert.Equal("Jane", model.Name);
            Assert.Equal("Doe", model.Surname);
            Assert.Equal("jane.doe@example.com", model.Email);
        }

        [Fact]
        public async Task Edit_Get_UnknownId_ReturnsNotFound()
        {
            _contactServiceMock.Setup(s => s.GetByIdAsync(99, _ct)).ReturnsAsync((ContactResponse?)null);

            var result = await _controller.Edit(99, _ct);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_UniqueEmail_UpdatesAndRedirects()
        {
            var request = MakeContactRequest(id: 5);
            _contactServiceMock.Setup(s => s.IsEmailUniqueAsync(request.Email, 5, _ct)).ReturnsAsync(true);

            var result = await _controller.Edit(request, _ct);

            _contactServiceMock.Verify(s => s.UpdateAsync(request, _ct), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirect.ActionName);
            Assert.Equal(5, redirect.RouteValues!["id"]);
        }


        [Fact]
        public async Task Edit_Post_DuplicateEmail_ReturnsView()
        {
            var request = MakeContactRequest(id: 5);
            _contactServiceMock.Setup(s => s.IsEmailUniqueAsync(request.Email, 5, _ct)).ReturnsAsync(false);
            _contactServiceMock.Setup(s => s.GetByIdAsync(5, _ct)).ReturnsAsync(MakeContactResponse(5));
            _clientServiceMock.Setup(s => s.GetAllAsync(_ct)).ReturnsAsync(new List<ClientResponse>());

            var result = await _controller.Edit(request, _ct);

            _contactServiceMock.Verify(s => s.UpdateAsync(It.IsAny<ContactRequest>(), _ct), Times.Never);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(request, viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
        }


        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsView()
        {
            _controller.ModelState.AddModelError("Name", "Required");
            var request = MakeContactRequest(id: 5);
            _contactServiceMock.Setup(s => s.IsEmailUniqueAsync(request.Email, 5, _ct)).ReturnsAsync(true);
            _contactServiceMock.Setup(s => s.GetByIdAsync(5, _ct)).ReturnsAsync(MakeContactResponse(5));
            _clientServiceMock.Setup(s => s.GetAllAsync(_ct)).ReturnsAsync(new List<ClientResponse>());

            var result = await _controller.Edit(request, _ct);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(request, viewResult.Model);
        }

        [Fact]
        public async Task LinkClient_NonAjax_LinksAndRedirectsToEdit()
        {
            var result = await _controller.LinkClient(1, 2, _ct);

            _contactServiceMock.Verify(s => s.LinkClientAsync(1, 2, _ct), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirect.ActionName);
        }

        [Fact]
        public async Task LinkClient_Ajax_LinksAndReturnsOk()
        {
            SetAjaxRequest();

            var result = await _controller.LinkClient(1, 2, _ct);

            _contactServiceMock.Verify(s => s.LinkClientAsync(1, 2, _ct), Times.Once);
            Assert.IsType<OkResult>(result);
        }


        [Fact]
        public async Task UnlinkClient_NonAjax_UnlinksAndRedirectsToEdit()
        {
            var result = await _controller.UnlinkClient(1, 2, _ct);

            _contactServiceMock.Verify(s => s.UnlinkClientAsync(1, 2, _ct), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirect.ActionName);
        }

        [Fact]
        public async Task UnlinkClient_Ajax_UnlinksAndReturnsOk()
        {
            SetAjaxRequest();

            var result = await _controller.UnlinkClient(1, 2, _ct);

            _contactServiceMock.Verify(s => s.UnlinkClientAsync(1, 2, _ct), Times.Once);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Delete_Get_KnownId_ReturnsViewWithContact()
        {
            var response = MakeContactResponse(3);
            _contactServiceMock.Setup(s => s.GetByIdAsync(3, _ct)).ReturnsAsync(response);

            var result = await _controller.Delete(3, _ct);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(response, viewResult.Model);
        }

        [Fact]
        public async Task Delete_Get_UnknownId_ReturnsNotFound()
        {
            _contactServiceMock.Setup(s => s.GetByIdAsync(99, _ct)).ReturnsAsync((ContactResponse?)null);

            var result = await _controller.Delete(99, _ct);

            Assert.IsType<NotFoundResult>(result);
        }

       
        [Fact]
        public async Task DeleteConfirmed_NonAjax_DeletesAndRedirectsToIndex()
        {
            var result = await _controller.DeleteConfirmed(3, _ct);

            _contactServiceMock.Verify(s => s.DeleteAsync(3, _ct), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task DeleteConfirmed_Ajax_DeletesAndReturnsOk()
        {
            SetAjaxRequest();

            var result = await _controller.DeleteConfirmed(3, _ct);

            _contactServiceMock.Verify(s => s.DeleteAsync(3, _ct), Times.Once);
            Assert.IsType<OkResult>(result);
        }
    }

}
