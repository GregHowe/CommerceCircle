using HotChocolate.Subscriptions;
using N1coLoyalty.Application.Common.Exceptions;
using N1coLoyalty.Application.Common.Models;

namespace N1coLoyalty.AdminApi.Common;

    public class MutationBase
    {
        internal async Task<PayloadResult<TRequest>> ResolveAsResponse<TRequest>(
            [Service] IMediator mediator,
            IRequest<CommonServiceResponse<TRequest>> command
        )
        {
            try
            {
                var result = await mediator.Send(command);

                return new PayloadResult<TRequest>
                {
                    Success = result.Success,
                    Code = result.Code,
                    Message = result.Message,
                    Data = result.Data
                };
            }
            catch (ValidationException ex)
            {
                var errors = ex.Errors;
                return new PayloadResult<TRequest>
                {
                    Code = "USER_ERROR",
                    Success = false,
                    Message = "Ocurrio un error",
                    UserErrors = errors.Keys.Select(k => new UserError
                    {
                        Field = string.IsNullOrEmpty(k) ? "general" : k,
                        Message = string.Join('\n', errors[k])
                    }).ToList()
                };
            }
        }

        internal async Task<PayloadResult> Execute<TRequest>(
            IRequest<TRequest> command,
            [Service] IMediator mediator,
            string successMessage = "Comando ejecutado satisfactoriamente",
            string errorMessage = "Ocurrio un error")
        {
            try
            {
                await mediator.Send(command);
                return new PayloadResult
                {
                    Success = true,
                    Message = successMessage
                };
            }
            catch (ValidationException ex)
            {
                var errors = ex.Errors;
                return new PayloadResult
                {
                    Success = false,
                    Message = errorMessage,
                    UserErrors = errors.Keys.Select(k => new UserError
                    {
                        Field = k,
                        Message = string.Join('\n', errors[k])
                    }).ToList()
                };
            }
        }

        internal async Task<PayloadResult> ExecuteWithSubscription<TRequest>(
            IRequest<TRequest> command,
            [Service] IMediator mediator,
            [Service] ITopicEventSender eventSender,
            string[] subcriptionsTypes,
            string successMessage = "Comando ejecutado satisfactoriamente",
            string errorMessage = "Ocurrio un error"
        )
        {
            try
            {
                await mediator.Send(command);

                foreach (var subscription in subcriptionsTypes) await eventSender.SendAsync(subscription, string.Empty);

                return new PayloadResult
                {
                    Success = true,
                    Message = successMessage
                };
            }
            catch (ValidationException ex)
            {
                var errors = ex.Errors;
                return new PayloadResult
                {
                    Success = false,
                    Message = errorMessage,
                    UserErrors = errors.Keys.Select(k => new UserError
                    {
                        Field = k,
                        Message = string.Join('\n', errors[k])
                    }).ToList()
                };
            }
        }
    }

