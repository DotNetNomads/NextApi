﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NextApi.Common;
using NextApi.Common.Abstractions.Security;
using NextApi.Common.Serialization;
using NextApi.Server.Request;
using NextApi.Server.Service;

namespace NextApi.Server.Base
{
    /// <summary>
    /// Request processor from HTTP (in case we don't want to use SignalR as transport)
    /// </summary>
    public class NextApiHttp
    {
        private readonly INextApiUserAccessor _userAccessor;
        private readonly INextApiRequest _request;
        private readonly NextApiHandler _handler;

        /// <inheritdoc />
        public NextApiHttp(INextApiUserAccessor userAccessor, INextApiRequest request, NextApiHandler handler)
        {
            _userAccessor = userAccessor ?? throw new ArgumentNullException(nameof(userAccessor));
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _handler = handler;
        }

        /// <summary>
        /// Entry point for HTTP requests
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task ProcessRequestAsync(HttpContext context)
        {
            var form = context.Request.Form;

            _userAccessor.User = context.User;
            _request.FilesFromClient = form.Files;

            var serializationType = GetSerializationType(form);

            var command = await CreateCommand(form, serializationType);

            var result = await _handler.ExecuteCommand(command);

            var resultWriter = CommandResultWriters.Create(result, serializationType);

            await resultWriter.Write(context.Response, result);
        }

        private async Task<NextApiCommand> CreateCommand(IFormCollection form, SerializationType serializationType)
        {
            var command = new NextApiCommand
            {
                Service = form["Service"].FirstOrDefault(),
                Method = form["Method"].FirstOrDefault()
            };

            var argsExtractor = CommandArgsExtractors.Create(serializationType);

            command.Args = await argsExtractor.Extract(form);

            return command;
        }

        private SerializationType GetSerializationType(IFormCollection form)
        {
            if (Enum.TryParse<SerializationType>(form["Serialization"], true, out var type))
            {
                return type;
            }

            return SerializationType.Json;
        }

        /// <summary>
        /// Returns list of supported permissions
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task GetSupportedPermissions(HttpContext context)
        {
            var permissions = _handler.GetSupportedPermissions();
            await context.Response.SendJson(permissions);
        }
    }
}
