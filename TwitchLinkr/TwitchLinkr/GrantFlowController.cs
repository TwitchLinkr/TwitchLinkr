using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class GrantFlowController : Controller
{
    private static TaskCompletionSource<string> authorizationCodeTcs = default!;
	private static TaskCompletionSource<string> authorizationScopeTcs = default!;
	private static TaskCompletionSource<string> authorizationStateTcs = default!;


	[HttpGet("/")]
    public IActionResult Callback(string code, string state, string scope, string error, string error_description)
    {
        if (!string.IsNullOrEmpty(error))
        {
            // Handle error
            authorizationCodeTcs.TrySetException(new Exception($"Error: {error_description}"));
            return Content($"Error: {error_description}");
        }

        // Set the authorization code
        authorizationCodeTcs.TrySetResult(code);
		authorizationScopeTcs.TrySetResult(scope);
		authorizationStateTcs.TrySetResult(state);
		return Content("Authorization code received. You can close this window.");
    }

    public static Task<(string code, string scope, string state)> WaitForAuthorizationCodeAsync()
    {
        authorizationCodeTcs = new TaskCompletionSource<string>();
		authorizationScopeTcs = new TaskCompletionSource<string>();
		authorizationStateTcs = new TaskCompletionSource<string>();
		return Task.WhenAll(authorizationCodeTcs.Task, authorizationScopeTcs.Task, authorizationStateTcs.Task).ContinueWith(t => (authorizationCodeTcs.Task.Result, authorizationScopeTcs.Task.Result, authorizationStateTcs.Task.Result));
	}
}
