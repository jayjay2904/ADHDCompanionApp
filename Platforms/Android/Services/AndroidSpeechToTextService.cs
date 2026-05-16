using ADHDCompanionApp.Services.Interfaces;
using Android.App;
using Android.Content;
using Android.Speech;
using Microsoft.Maui.ApplicationModel;

namespace ADHDCompanionApp.Platforms.Android.Services;

public class AndroidSpeechToTextService : ISpeechToTextService
{
    public async Task<string?> ListenAsync()
    {
        var permission = await Permissions.CheckStatusAsync<Permissions.Microphone>();

        if (permission != PermissionStatus.Granted)
        {
            permission = await Permissions.RequestAsync<Permissions.Microphone>();
        }

        if (permission != PermissionStatus.Granted)
            return null;

        var activity = Platform.CurrentActivity;

        if (activity is null)
            return null;

        var intent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
        intent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
        intent.PutExtra(RecognizerIntent.ExtraPrompt, "Talk to Arlo");
        intent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);

        var result = await SpeechRecognitionActivity.StartAsync(activity, intent);

        return result;
    }
}

public class SpeechRecognitionActivity
{
    private const int RequestCode = 9001;
    private static TaskCompletionSource<string?>? _tcs;

    public static Task<string?> StartAsync(Activity activity, Intent intent)
    {
        _tcs = new TaskCompletionSource<string?>();

        activity.StartActivityForResult(intent, RequestCode);

        return _tcs.Task;
    }

    public static void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        if (requestCode != RequestCode)
            return;

        if (resultCode != Result.Ok || data is null)
        {
            _tcs?.TrySetResult(null);
            return;
        }

        var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
        var text = matches?.FirstOrDefault();

        _tcs?.TrySetResult(text);
    }
}