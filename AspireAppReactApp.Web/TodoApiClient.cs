using System.Net.Http;

namespace AspireAppReactApp.Web
{
    public class TodoApiClient(HttpClient httpClient)
    {
        public async Task<List<TodoDto>> GetTodosAsync(CancellationToken cancellationToken = default)
       => await httpClient.GetFromJsonAsync<List<TodoDto>>("/Todo", cancellationToken) ?? [];

        public async Task<TodoDto?> GetTodoByIdAsync(int id, CancellationToken cancellationToken = default)
            => await httpClient.GetFromJsonAsync<TodoDto>($"/Todo/{id}", cancellationToken);

        public async Task<TodoDto?> AddTodoAsync(TodoDto todo, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.PostAsJsonAsync("/Todo", todo, cancellationToken);
            return await response.Content.ReadFromJsonAsync<TodoDto>(cancellationToken: cancellationToken);
        }

        public async Task<bool> UpdateTodoAsync(int id, TodoDto todo, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.PutAsJsonAsync($"/Todo/{id}", todo, cancellationToken);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteTodoAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.DeleteAsync($"/Todo/{id}", cancellationToken);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SwapTodoPositionAsync(int id1, int id2, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.PostAsync($"/Todo/swap-position/{id1}/{id2}", null, cancellationToken);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> MoveTaskUpAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.PostAsync($"/Todo/move-up/{id}", null, cancellationToken);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> MoveTaskDownAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.PostAsync($"/Todo/move-down/{id}", null, cancellationToken);
            return response.IsSuccessStatusCode;
        }
    }
}
