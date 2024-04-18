﻿using CRM.Core.Dtos;
using CRM.Core.Models;
using CRM.Core.Services.Interfaces;
using System.Security.Claims;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CRM.Core.Services.Implementations
{
    public class SalesService : ISalesRepresntative
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISharedService _sharedService;


        public SalesService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ISharedService sharedService)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _sharedService = sharedService;

        }

        #region ManageCalls
        

        //public async Task<ResultDto> UpdateCallInfo(CallDto callDto, string callId)
        //{
        //    var call = await _unitOfWork.Calls.FindAsync(c => c.CallID == callId);
        //    if (call == null)
        //    {
        //        return new ResultDto
        //        {
        //            IsSuccess = false,
        //            Errors=["Call not Found"]
        //        };
        //    }
        //    var customer = await _unitOfWork.Customers.GetByIdAsync(callDto.CustomerId);
        //    if (customer == null)
        //    {
        //        return new ResultDto
        //        {
        //            IsSuccess = false,
        //            Errors = ["Customer not Found"]
        //        };
        //    }
        //    call.CallDate = callDto.date;
        //    call.CallStatus = callDto.status;
        //    call.CallSummery = callDto.summary;
        //    call.Customer = customer;
        //    call.FollowUpDate = callDto.followUp;

        //    try
        //    {
        //        _unitOfWork.complete();
        //    }
        //    catch (Exception e)
        //    {
        //        return new ResultDto
        //        {
        //            IsSuccess = false,
        //           Errors = [e.Message]
        //        };
        //    }
        //    return new ResultDto
        //    {
        //        IsSuccess = true,
        //        Message = "Call updated successfully"
        //    };
        //}
        public async Task<ActionResult<IEnumerable<object>>> AddCall(AddCallDto addcallDto, string salesRepEmail)
        {
            try
            {
                if (addcallDto == null)
                {
                    throw new ArgumentNullException(nameof(addcallDto));
                }

                var calls = new List<object>();

                var customer = await _unitOfWork.Customers.GetByIdAsync(addcallDto.CustomerId);
                if (customer == null)
                {
                    return new BadRequestObjectResult(new { errors = new List<string> { "Customer not found" } });
                }

                if (string.IsNullOrEmpty(salesRepEmail))
                {
                    return new BadRequestObjectResult(new { errors = new List<string> { "Sales Representative email is null or empty" } });
                }

                var salesRep = await _unitOfWork.UserManager.FindByEmailAsync(salesRepEmail);
                if (salesRep == null)
                {
                    return new BadRequestObjectResult(new { errors = new List<string> { "Sales Representative not found" } });
                }

                var call = new Call
                {
                    CallStatus = addcallDto.status,
                    CallSummery = addcallDto.summary,
                    CallDate = addcallDto.date,
                    FollowUpDate = addcallDto.followUp,
                    SalesRepresntative = salesRep,
                    Customer = customer
                };

                await _unitOfWork.Calls.AddAsync(call);
                try
                {
                    _unitOfWork.complete();
                }
                catch (Exception e)
                {
                    return new BadRequestObjectResult(new { errors = new List<string> { e.Message } });
                }

                calls.Add(new CallDto
                {
                    id = call.CallID,
                    status = call.CallStatus,
                    summary = call.CallSummery,
                    date = call.CallDate,
                    followUp = call.FollowUpDate,
                    CustomerId = call.Customer.CustomerId
                });

                return new OkObjectResult(calls);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new { errors = new List<string> { ex.Message } });
            }
        }







        public async Task<List<CallDto>> UpdateCallInfo(AddCallDto addCallDto, string callId)
        {
            var call = await _unitOfWork.Calls.FindAsync(c => c.CallID == callId);
            if (call == null)
            {
                // If call not found, return null or an empty list depending on your preference
                return null;
            }

            // Update CustomerId if provided
            if (addCallDto.CustomerId != 0)
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(addCallDto.CustomerId);
                if (customer == null)
                {
                    // If customer not found, handle accordingly, here I'm returning null
                    return null;
                }

                // Update CustomerId and Customer navigation property
                call.Customer.CustomerId = addCallDto.CustomerId;
                call.Customer = customer;
            }

            // Update other call properties
            call.CallStatus = addCallDto.status;
            call.CallSummery = addCallDto.summary;
            call.CallDate = addCallDto.date;
            call.FollowUpDate = addCallDto.followUp;

            try
            {
                _unitOfWork.complete(); // Assuming asynchronous completion
            }
            catch (Exception e)
            {
                // Handle exceptions, here I'm returning null
                Debug.WriteLine($"Error: {e.Message}");
                return null;
            }

            // Return updated call information
            return new List<CallDto>
    {
        new CallDto
        {
            // Populate your CallDto properties here
            id = call.CallID,
            status = call.CallStatus,
            summary = call.CallSummery,
            date = call.CallDate,
            followUp = call.FollowUpDate,
            CustomerId = call.Customer.CustomerId,

        }
    };
        }


        //public async Task<ReturnCallsDto> GetAllCalls()
        //{
        //    try
        //    {
        //        var includes = new string[] { "Customer" }; 
        //        var calls = await _unitOfWork.Calls.GetAllAsync(call => true, int.MaxValue, 0, includes);

        //        if (calls == null || !calls.Any())
        //        {
        //            return new ReturnCallsDto
        //            {
        //                IsSuccess = false,
        //                Errors = ["Calls not Found"]

        //            };
        //        }

        //        var Calls = new List<CallDto>();
        //        foreach (var call in calls)
        //        {
        //            var callDto = new CallDto
        //            {
        //                id= call.CallID,
        //                type = ActionType.call,
        //                status = call.CallStatus,
        //                summary = call.CallSummery,
        //                date = call.CallDate,
        //                followUp = call.FollowUpDate,
        //             // CustomerId = call.Customer != null ? call.Customer.CustomerId : 0

        //            };
        //            Calls.Add(callDto);
        //        }


        //        return new ReturnCallsDto
        //        {
        //            IsSuccess = true,
        //            Message = "Calls found",
        //            Calls = Calls
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ReturnCallsDto
        //        {
        //            IsSuccess = false,
        //            Errors=[ex.Message]
        //        };
        //    }
        //}
        public async Task<IEnumerable<CallDto>> GetAllCalls()
        {
            try
            {
                var includes = new string[] { "Customer" };
                var calls = await _unitOfWork.Calls.GetAllAsync(call => true, int.MaxValue, 0, includes);

                if (calls == null || !calls.Any())
                {
                    return null; 
                }

                var Calls = new List<CallDto>();
                foreach (var call in calls)
                {
                    var callDto = new CallDto
                    {
                        id = call.CallID,
                        type = ActionType.call,
                        status = call.CallStatus,
                        summary = call.CallSummery,
                        date = call.CallDate,
                        followUp = call.FollowUpDate,
                        CustomerId = call.Customer != null ? call.Customer.CustomerId : 0,


                    };
                    Calls.Add(callDto);
                }

                return Calls;
            }
            catch 
            {
                
                return Enumerable.Empty<CallDto>(); 
            }
        }

        public async Task<IEnumerable<CallDto>> GetCallById(string callId)
        {
            try
            {
                var includes = new string[] { "Customer" };
                var call = await _unitOfWork.Calls.FindAsync(c => c.CallID == callId, includes);

                if (call == null)
                {
                    return null ; 
                }

                var callDto = new CallDto
                {
                    id = call.CallID,
                    type = ActionType.call,
                    status = call.CallStatus,
                    summary = call.CallSummery,
                    date = call.CallDate,
                    followUp = call.FollowUpDate,
                    CustomerId = call.Customer != null ? call.Customer.CustomerId : 0,
                };

                return new List<CallDto> { callDto };
            }
            catch 
            {
                
                return Enumerable.Empty<CallDto>(); 
            }
        }

        public async Task<ResultDto> DeleteCallById(string callId)
        {
            try
            {
                var call = await _unitOfWork.Calls.FindAsync(c => c.CallID == callId);
                if (call == null)
                {
                    return new ResultDto
                    {
                        IsSuccess = false,
                        Errors = ["Call not Found"]
                    };
                }

                _unitOfWork.Calls.Delete(call);
                try
                {
                    _unitOfWork.complete();
                }
                catch (Exception e)
                {
                    return new ResultDto
                    {
                        IsSuccess = false,
                       Errors = [e.Message] 
                    };
                }

                return new ResultDto
                {
                    IsSuccess = true,
                    Message =  "Call deleted successfully" 
                };
            }
            catch (Exception ex)
            {
                return new ResultDto
                {
                    IsSuccess = false,
                    Errors = [ ex.Message]
                };
            }
        }

        #endregion
        #region ManageMessages
        public async Task<ActionResult<IEnumerable<object>>> AddMessage(AddMessageDto messageDto, string salesRepEmail)
        {
            try
            {
                if (messageDto == null)
                {
                    throw new ArgumentNullException(nameof(messageDto));
                }

                

                var customer = await _unitOfWork.Customers.GetByIdAsync(messageDto.CustomerId);
                if (customer == null)
                {
                    return new BadRequestObjectResult(new { errors = new List<string> { "Customer not found" } });
                }

                if (string.IsNullOrEmpty(salesRepEmail))
                {
                    return new BadRequestObjectResult(new { errors = new List<string> { "Sales Representative email is null or empty" } });
                }

                var salesRep = await _unitOfWork.UserManager.FindByEmailAsync(salesRepEmail);
                if (salesRep == null)
                {
                    return new BadRequestObjectResult(new { errors = new List<string> { "Sales Representative not found" } });
                }


                var message = new Message
                {
                   
                    MessageContent = messageDto.summary,
                    MessageDate = messageDto.date,
                    FollowUpDate = messageDto.followUp,
                    SalesRepresntative = salesRep,
                    Customer = customer
                };

                await _unitOfWork.Messages.AddAsync(message);
                try
                {
                    _unitOfWork.complete();
                }
                catch (Exception e)
                {
                    return new BadRequestObjectResult(new { errors = new List<string> { e.Message } });
                }
                var messages = new List<object>();

                messages.Add(new MessageDto
                {
                    id = message.MessageID,
                    summary = message.MessageContent,
                    date = message.MessageDate,
                    followUp = message.FollowUpDate,
                    CustomerId = message.Customer.CustomerId
                });

                return new OkObjectResult(messages);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new { errors = new List<string> { ex.Message } });
            }
        }

        public async Task<ResultDto> UpdateMessageInfo(AddMessageDto messageDto, string MessageId)
        {
            var message = await _unitOfWork.Messages.FindAsync(c => c.MessageID == MessageId);
            if (message == null)
            {
                return new ResultDto
                {
                    IsSuccess = false,
                    Errors = ["Message not Found"]
                };
            }
            var customer = await _unitOfWork.Customers.GetByIdAsync(messageDto.CustomerId);
            if (customer == null)
            {
                return new ResultDto
                {
                    IsSuccess = false,
                    Errors = ["Customer not Found"]

                };
            }

            //var message  = new Message { 
            //message.MessageContent = messageDto.summary;
            //message.MessageDate = messageDto.date;
            //message.FollowUpDate = messageDto.followUp;
            //message.Customer.CustomerId = messageDto.CustomerId;
            try
            {
                _unitOfWork.complete();
            }
            catch (Exception e)
            {
                return new ResultDto
                {
                    IsSuccess = false,
                    Errors = [e.Message]
                };
            }
            return new ResultDto
            {
                IsSuccess = true,
                Message = "Message updated successfully"
            };
        }



        public async Task<IEnumerable<MessageDto>> GetAllMessages()
        {
            try
            {
                var includes = new string[] { "Customer" };
                var messages = await _unitOfWork.Messages.GetAllAsync(message => true, int.MaxValue, 0, includes);

                if (messages == null || !messages.Any())
                {
                    return null ;
                }

                var Messages = messages.Select(message => new MessageDto
                {
                    id = message.MessageID,
                    type = ActionType.message,
                    summary = message.MessageContent,
                    date = message.MessageDate,
                    followUp = message.FollowUpDate,
                    CustomerId = message.Customer != null ? message.Customer.CustomerId : 0
                });

                return Messages;
            }
            catch 
            {
               
                throw; 
            }
        }


        public async Task<IEnumerable<MessageDto>> GetMessageById(string messageID)
        {
            try
            {
                var includes = new string[] { "Customer" };
                var message = await _unitOfWork.Messages.FindAsync(c => c.MessageID == messageID, includes);

                if (message == null)
                {
                    return null;
                }

                var messageDto = new MessageDto
                {
                    id = message.MessageID,
                    type = ActionType.message,
                    summary = message.MessageContent,
                    date = message.MessageDate,
                    followUp = message.FollowUpDate,
                    CustomerId = message.Customer != null ? message.Customer.CustomerId : 0
                };

                return new List<MessageDto> { messageDto };
            }
            catch 
            {
                
                throw; 
            }
        }


        public async Task<ResultDto> DeleteMessageById(string MessageId)
        {
            try
            {
                var message = await _unitOfWork.Messages.FindAsync(c => c.MessageID == MessageId);
                if (message == null)
                {
                    return new ResultDto
                    {
                        IsSuccess = false,
                        Errors = ["Message not Found"]
                    };
                }

                _unitOfWork.Messages.Delete(message);
                try
                {
                    _unitOfWork.complete();
                }
                catch (Exception e)
                {
                    return new ResultDto
                    {
                        IsSuccess = false,
                       Errors = [e.Message]
                    };
                }

                return new ResultDto
                {
                    IsSuccess = true,
                    Message = " Message deleted successfully"
                };
            }
            catch (Exception ex)
            {
                return new ResultDto
                {
                    IsSuccess = false,
                    Errors = [ex.Message]   
                };
            }
        }

        #endregion

        #region ManageMeetings
        public async Task<ActionResult<IEnumerable<object>>> AddMeeting(AddMeetingDto meetingDto, string SaleRepEmail)
        {
            try
            {
                if (meetingDto == null)
                {
                    throw new ArgumentNullException(nameof(MeetingDto));
                }
                
                var customer = await _unitOfWork.Customers.GetByIdAsync(meetingDto.CustomerId);
                if (customer == null)
                {
                    return new BadRequestObjectResult(new { errors = new List<string> { "Customer not found" } });

                }

                if (string.IsNullOrEmpty(SaleRepEmail))
                {
                    return new BadRequestObjectResult(new { errors = new List<string> { "Sales Representative email is null or empty" } });
                }

                var salesRep = await _unitOfWork.UserManager.FindByEmailAsync(SaleRepEmail);

                if (salesRep == null)
                {
                    return new BadRequestObjectResult(new { errors = new List<string> { "Sales Representative not found" } });

                }


                var meeting = new Meeting
                {
                    connectionState = meetingDto.online,
                    SalesRepresntative = salesRep,
                    Customer = customer,
                    FollowUpDate = meetingDto.followUp,
                    MeetingDate = meetingDto.date,
                    MeetingSummary = meetingDto.summary
                };

                await _unitOfWork.Meetings.AddAsync(meeting);
                var meetings = new List<object>();

                try
                {
                    _unitOfWork.complete();
                }

                catch (Exception e)
                {
                    return new BadRequestObjectResult(new { errors = new List<string> { e.Message } });
                }

                meetings.Add(new MeetingDto
                {
                    id = meeting.MeetingID,
                    summary = meeting.MeetingSummary,
                    date = meeting.MeetingDate,
                    followUp = meeting.FollowUpDate,
                    CustomerId = meeting.Customer.CustomerId
                });


                return new OkObjectResult(meetings);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new { errors = new List<string> { ex.Message } });
            }
        }
        public async Task<ResultDto> UpdateMeeting(AddMeetingDto meetingDto, string MeetingId)
        {
            var meeting = await _unitOfWork.Meetings.FindAsync(c => c.MeetingID == MeetingId);
            if (meeting == null)
            {
                return new ResultDto
                {
                    IsSuccess = false,
                    Errors = ["Meeting not Found"]
                };
            }
            var customer = await _unitOfWork.Customers.GetByIdAsync(meetingDto.CustomerId);
            if (customer == null)
            {
                return new ResultDto
                {
                    IsSuccess = false,
                    Errors = ["Customer not Found"]
                };
            }
            meeting.connectionState = meetingDto.online;
            meeting.Customer = customer;
            meeting.FollowUpDate = meetingDto.followUp;
            meeting.MeetingSummary = meetingDto.summary;
            try
            {
                _unitOfWork.complete();
            }
            catch (Exception e)
            {
                return new ResultDto
                {
                    IsSuccess = false,
                    Errors = [e.Message]
                };
            }
            return new ResultDto
            {
                IsSuccess = true,
                Message = "Meeting updated successfully"
            };
        }

        public async Task<IEnumerable<MeetingDto>> GetAllMeetings()
        {
            try
            {
                var includes = new string[] { "Customer" };
                var meetings = await _unitOfWork.Meetings.GetAllAsync(meetings => true, int.MaxValue, 0, includes);

                if (meetings == null || !meetings.Any())
                {
                    return Enumerable.Empty<MeetingDto>();
                }

                var Meetings = meetings.Select(meeting => new MeetingDto
                {
                    id = meeting.MeetingID,
                    type = ActionType.meeting,
                    online = meeting.connectionState,
                    summary = meeting.MeetingSummary,
                    date = meeting.MeetingDate,
                    followUp = meeting.FollowUpDate,
                    CustomerId = meeting.Customer != null ? meeting.Customer.CustomerId : 0 
                });

                return Meetings;
            }
            catch 
            {
                
                throw; 
            }
        }


        public async Task<IEnumerable<MeetingDto>> GetMeetingByID(string MeetingId)
        {
            try
            {
                var includes = new string[] { "Customer" };
                var meeting = await _unitOfWork.Meetings.FindAsync(c => c.MeetingID == MeetingId, includes);

                if (meeting == null)
                {
                    return Enumerable.Empty<MeetingDto>();
                }

                var meetingDto = new MeetingDto
                {
                    id = meeting.MeetingID,
                    type = ActionType.meeting,
                    online = meeting.connectionState,
                    summary = meeting.MeetingSummary,
                    date = meeting.MeetingDate,
                    followUp = meeting.FollowUpDate,
                    
                    CustomerId = meeting.Customer != null ? meeting.Customer.CustomerId : 0 
                };

                return new List<MeetingDto> { meetingDto };
            }
            catch (Exception ex)
            {
               
                throw; 
            }
        }

        public async Task<ResultDto> DeleteMeeting(string MeetingId)
        {
            try
            {
                var meeting = await _unitOfWork.Meetings.FindAsync(c => c.MeetingID == MeetingId);
                if (meeting == null)
                {
                    return new ResultDto
                    {
                        IsSuccess = false,
                        Errors = ["Meeting not Found"]
                    };
                }

                _unitOfWork.Meetings.Delete(meeting);
                try
                {
                    _unitOfWork.complete();
                }
                catch (Exception e)
                {
                    return new ResultDto
                    {
                        IsSuccess = false,
                        Errors = [e.Message]
                    };
                }

                return new ResultDto
                {
                    IsSuccess = true,
                    Message = "Meeting is deleted successfully"
                };
            }
            catch (Exception ex)
            {
                return new ResultDto
                {
                    IsSuccess = false,
                    Errors = [ex.Message]
                };
            }
        }
        #endregion

        #region ManageDeals
        public async Task<ActionResult<IEnumerable<object>>> AddDeal(AddDealDto dealsDto, string salesRepEmail)
        {
            try
            {
                if (dealsDto == null)
                {
                    throw new ArgumentNullException(nameof(dealsDto));
                }

                var deals = new List<object>();

                var customer = await _unitOfWork.Customers.GetByIdAsync(dealsDto.CustomerId);
                if (customer == null)
                {
                    return new BadRequestObjectResult(new { errors = new List<string> { "Customer Not Found" } });

                }
                var interest = await _unitOfWork.Interests.GetByIdAsync(dealsDto.InterestId);
                if (interest == null)
                {
                    return new BadRequestObjectResult(new { errors = new List<string> { "Interest Not Found" } });
                }

                if (string.IsNullOrEmpty(salesRepEmail))
                {
                    return new BadRequestObjectResult(new { errors = new List<string> { "Sales Representative email is null or empty" } });
                }

                var salesRep = await _unitOfWork.UserManager.FindByEmailAsync(salesRepEmail);
                if (salesRep == null)
                {
                    return new BadRequestObjectResult(new { errors = new List<string> { "Sales Representative not found " } });
                }
                var deal = new Deal
                {
                    DealDate = dealsDto.date,
                description = dealsDto.summary,
                Price = dealsDto.price,
               Interest = interest,
                SalesRepresntative = salesRep,
               Customer = customer,
               };
                await _unitOfWork.Deals.AddAsync(deal);

                try
                {
                    _unitOfWork.complete();
                }
                catch (Exception e)
                {
                    return new BadRequestObjectResult(new { errors = new List<string> { e.Message } });
                }
              

                deals.Add(new DealsDto
                {
                    id = deal.DealId,
                    price = deal.Price,
                    InterestId=deal.Interest.InterestID,
                    summary = deal.description,
                    date = deal.DealDate,
                });

                return new OkObjectResult(deals);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new { errors = new List<string> { ex.Message } });
            }
        }



        public async Task<ResultDto> UpdateDeal(AddDealDto dealsDto, string dealId)
        {
            var deal = await _unitOfWork.Deals.FindAsync(c => c.DealId == dealId);
            if (deal == null)
            {
                return new ResultDto
                {
                    IsSuccess = false,
                    Errors = ["Deal not found"]
                };
            }
            var customer = await _unitOfWork.Customers.GetByIdAsync(dealsDto.CustomerId);
            if (customer == null)
            {
                return new ResultDto
                {
                    IsSuccess = false,
                    Errors = ["Customer not found"]
                };
            }
            var interest = await _unitOfWork.Interests.GetByIdAsync(dealsDto.InterestId);
            if (interest == null)
            {
                return new ResultDto
                {
                    IsSuccess = false,
                    Errors = ["Interest not found"]
                };
            }

            deal.Price = dealsDto.price;
            deal.DealDate = dealsDto.date;
            deal.description = dealsDto.summary;
            deal.Interest = interest;
            deal.Customer = customer;

            try
            {
                _unitOfWork.complete();
            }
            catch (Exception e)
            {
                return new ResultDto
                {
                    IsSuccess = false,
                    Errors = [e.Message]
                };
            }
            return new ResultDto
            {
                IsSuccess = true,
                Message = "Deal updated successfully"
            };
        }






        public async Task<IEnumerable<DealsDto>> GetAllDeals()
        {
            try
            {
                var includes = new string[] { "Customer", "Interest" };
                var deals = await _unitOfWork.Deals.GetAllAsync(call => true, int.MaxValue, 0, includes);

                if (deals == null || !deals.Any())
                {
                    return Enumerable.Empty<DealsDto>();
                }

                var Deals = deals.Select(deal => new DealsDto
                {
                    id = deal.DealId,
                    type = ActionType.deal,
                    price = deal.Price,
                    interest = new InterestDto
                    {
                        Id = deal.Interest.InterestID,
                        Name = deal.Interest.InterestName
                    },
                    summary = deal.description,
                    date = deal.DealDate
                });

                return Deals;
            }
            catch
            {
                
                throw; 
            }
        }

        public async Task<IEnumerable<DealsDto>> GetDealById(string DealId)
        {
            try
            {
                var includes = new string[] { "Customer", "Interest" };
                var deal = await _unitOfWork.Deals.FindAsync(c => c.DealId == DealId, includes);

                if (deal == null)
                {
                    return Enumerable.Empty<DealsDto>();
                }

                var dealDto = new DealsDto
                {
                    id = deal.DealId,
                    type = ActionType.deal,
                    price = deal.Price,
                    interest = new InterestDto
                    {
                        Id = deal.Interest.InterestID,
                        Name = deal.Interest.InterestName
                    },
                    summary = deal.description,
                    date = deal.DealDate
                };

                return new List<DealsDto> { dealDto };
            }
            catch (Exception ex)
            {
                
                throw; 
            }
        }

        public async Task<ResultDto> DeleteDeal(string dealId)
        {
            try
            {
                var deal = await _unitOfWork.Deals.FindAsync(c => c.DealId == dealId);
                if (deal == null)
                {
                    return new ResultDto
                    {
                        IsSuccess = false,
                        Errors =[ "Deal not found"]
                    };
                }

                _unitOfWork.Deals.Delete(deal);
                try
                {
                    _unitOfWork.complete();
                }
                catch (Exception e)
                {
                    return new ResultDto
                    {
                        IsSuccess = false,
                        Errors =[e.Message]
                    };
                }

                return new ResultDto
                {
                    IsSuccess = true,
                    Message = "Deal deleted successfully"
                };
            }
            catch (Exception ex)
            {
                return new ResultDto
                {
                    IsSuccess = false,
                    Errors =[ex.Message]
                };
            }
        }
        #endregion

     
        public async Task<ActionResult<IEnumerable<object>>> GetAllActionsForCustomer(int customerId)
        {
            var salesRepresentativeEmail = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email);
            var salesRepresentative = await _unitOfWork.UserManager.FindByEmailAsync(salesRepresentativeEmail);

            if (salesRepresentative == null)
            {
                return new BadRequestObjectResult(new { errors = new List<string> { "Sales representative not found" } });
            }

            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);

            if (customer == null)
            {
                return new BadRequestObjectResult(new { errors = new List<string> { "Customer not found" } });
            }

            if (customer.SalesRepresntative == null || customer.SalesRepresntative.Id != salesRepresentative.Id)
            {
                return new BadRequestObjectResult( new { errors = new List<string> { "This customer is not assigned to you" } });
            }

            var actions = await _sharedService.GetActionsForCustomer(customerId);

            return new OkObjectResult(actions);
        }







    }
}









    

