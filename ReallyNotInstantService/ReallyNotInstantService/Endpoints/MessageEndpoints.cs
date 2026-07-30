namespace ReallyNotInstantService.Endpoints {
    public static class MessageEndpoints {
        public static void MapMessageEndpoints(this IEndpointRouteBuilder app) {
            SendMessageEndpoint.MapSendMessage(app);
            GetMessageStatusEndpoint.MapGetMessageStatus(app);
        }
    }
}
