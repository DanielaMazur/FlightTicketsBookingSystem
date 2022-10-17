using System;
using System.Threading.Tasks;
using AdminService.Interfaces;
using AdminService.Models;
using AdminService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace AdminServiceUnitTests
{
     public class TicketsTests
     {
          private Mock<ICosmosDbService<Ticket>> _cosmosDbServiceMock;
          private Mock<ILogger<ITicketsService>> _loggerMock;
          private ITicketsService _ticketsService;

          [SetUp]
          public void Setup()
          {
               _cosmosDbServiceMock = new Mock<ICosmosDbService<Ticket>>();
               _loggerMock = new Mock<ILogger<ITicketsService>>();
               _ticketsService = new TicketsService(_loggerMock.Object, _cosmosDbServiceMock.Object);
          }

          [Test]
          public async Task CreateTicket_Success()
          {
               // arrange
               var ticket = new Ticket()
               {
                    FromAirportId = Guid.NewGuid().ToString(),
                    ToAirportId = Guid.NewGuid().ToString(),
                    DepartureTime = DateTime.Now.AddDays(2),
                    Price = 100,
               };

               var ticketWithId = new Ticket()
               {
                    FromAirportId = ticket.FromAirportId,
                    ToAirportId = ticket.ToAirportId,
                    DepartureTime = ticket.DepartureTime,
                    Price = ticket.Price,
                    Id = Guid.NewGuid().ToString()
               }; ;
               _cosmosDbServiceMock.Setup(m => m.AddAsync(ticket)).ReturnsAsync(ticketWithId);

               // act
               var result = await _ticketsService.CreateTicket(ticket);

               // assert
               Assert.IsNotNull(result);
               Assert.IsNotNull(result.Id);
               Assert.AreEqual(result.FromAirportId, ticket.FromAirportId);
               Assert.AreEqual(result.ToAirportId, ticket.ToAirportId);
               Assert.AreEqual(result.DepartureTime, ticket.DepartureTime);
               Assert.AreEqual(result.Price, ticket.Price);
          }

          [Test]
          public void CreateTicket_WithoutFromAirport()
          {
               // arrange
               var ticket = new Ticket()
               {
                    ToAirportId = Guid.NewGuid().ToString(),
                    DepartureTime = DateTime.Now.AddDays(2),
                    Price = 100,
               };

               var ticketWithId = new Ticket()
               {
                    ToAirportId = ticket.ToAirportId,
                    DepartureTime = ticket.DepartureTime,
                    Price = ticket.Price,
                    Id = Guid.NewGuid().ToString()
               }; ;
               _cosmosDbServiceMock.Setup(m => m.AddAsync(ticket)).ReturnsAsync(ticketWithId);

               // act
               // assert
               Assert.ThrowsAsync<BadHttpRequestException>(async () => await _ticketsService.CreateTicket(ticket));
          }

          [Test]
          public void CreateTicket_WithoutToAirport()
          {
               // arrange
               var ticket = new Ticket()
               {
                    FromAirportId = Guid.NewGuid().ToString(),
                    DepartureTime = DateTime.Now.AddDays(2),
                    Price = 100,
               };

               var ticketWithId = new Ticket()
               {
                    FromAirportId = ticket.FromAirportId,
                    DepartureTime = ticket.DepartureTime,
                    Price = ticket.Price,
                    Id = Guid.NewGuid().ToString()
               }; ;
               _cosmosDbServiceMock.Setup(m => m.AddAsync(ticket)).ReturnsAsync(ticketWithId);

               // act
               // assert
               Assert.ThrowsAsync<BadHttpRequestException>(async () => await _ticketsService.CreateTicket(ticket));
          }


          [Test]
          public void CreateTicket_WithEqualToAndFromAirports()
          {
               // arrange
               var ticket = new Ticket()
               {
                    ToAirportId = "20b99c83-5afd-45df-b921-c9abf0bf593e",
                    FromAirportId = "20b99c83-5afd-45df-b921-c9abf0bf593e",
                    DepartureTime = DateTime.Now.AddDays(2),
                    Price = 100,
               };

               var ticketWithId = new Ticket()
               {
                    ToAirportId = ticket.ToAirportId,
                    FromAirportId = ticket.FromAirportId,
                    DepartureTime = ticket.DepartureTime,
                    Price = ticket.Price,
                    Id = Guid.NewGuid().ToString()
               }; ;
               _cosmosDbServiceMock.Setup(m => m.AddAsync(ticket)).ReturnsAsync(ticketWithId);

               // act
               // assert
               Assert.ThrowsAsync<BadHttpRequestException>(async () => await _ticketsService.CreateTicket(ticket));
          }

          [Test]
          public async Task UpdateTicket_Success()
          {
               // arrange
               var ticketId = Guid.NewGuid().ToString();
               var ticket = new Ticket()
               {
                    ToAirportId = Guid.NewGuid().ToString(),
                    FromAirportId = Guid.NewGuid().ToString(),
                    DepartureTime = DateTime.Now.AddDays(2),
                    Price = 100,
                    Id = ticketId
               };

               var updateTicket = new UpdateTicket()
               {
                    DepartureTime = DateTime.Now.AddDays(7),
                    Price = 200
               };

               _cosmosDbServiceMock.Setup(m => m.GetAsync(ticketId)).ReturnsAsync(ticket);
               _cosmosDbServiceMock.Setup(m => m.UpdateAsync(ticketId, ticket));

               // act
               var result = await _ticketsService.UpdateTicket(ticketId, updateTicket);

               // assert
               Assert.IsNotNull(result);
               Assert.AreEqual(result.FromAirportId, ticket.FromAirportId);
               Assert.AreEqual(result.ToAirportId, ticket.ToAirportId);
               Assert.AreEqual(result.DepartureTime, updateTicket.DepartureTime);
               Assert.AreEqual(result.Price, updateTicket.Price);
          }

          [Test]
          public void UpdateTicket_WithoutId()
          {
               // arrange
               string ticketId = null;
               var ticket = new Ticket()
               {
                    ToAirportId = Guid.NewGuid().ToString(),
                    FromAirportId = Guid.NewGuid().ToString(),
                    DepartureTime = DateTime.Now.AddDays(2),
                    Price = 100,
                    Id = ticketId
               };

               var updateTicket = new UpdateTicket()
               {
                    DepartureTime = DateTime.Now.AddDays(7),
                    Price = 200
               };

               _cosmosDbServiceMock.Setup(m => m.GetAsync(ticketId)).ReturnsAsync(ticket);
               _cosmosDbServiceMock.Setup(m => m.UpdateAsync(ticketId, ticket));

               // act
               // assert
               Assert.ThrowsAsync<BadHttpRequestException>(async () => await _ticketsService.UpdateTicket(ticketId, updateTicket));
          }

          [Test]
          public void UpdateTicket_UnexistingTicket()
          {
               // arrange
               string ticketId = Guid.NewGuid().ToString();
               var ticket = new Ticket()
               {
                    ToAirportId = Guid.NewGuid().ToString(),
                    FromAirportId = Guid.NewGuid().ToString(),
                    DepartureTime = DateTime.Now.AddDays(2),
                    Price = 100,
                    Id = ticketId
               };

               var updateTicket = new UpdateTicket()
               {
                    DepartureTime = DateTime.Now.AddDays(7),
                    Price = 200
               };

               _cosmosDbServiceMock.Setup(m => m.GetAsync(ticketId)).ReturnsAsync((Ticket)null);
               _cosmosDbServiceMock.Setup(m => m.UpdateAsync(ticketId, ticket));

               // act
               // assert
               Assert.ThrowsAsync<BadHttpRequestException>(async () => await _ticketsService.UpdateTicket(ticketId, updateTicket));
          }
     }
}