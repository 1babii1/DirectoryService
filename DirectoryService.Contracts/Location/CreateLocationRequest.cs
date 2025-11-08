namespace DirectoryService.Contracts.Location;

public record CreateLocationRequest(string Name, Address Address, string Timezone);