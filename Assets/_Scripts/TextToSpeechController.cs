using IBM.Watson.TextToSpeech.V1;
using IBM.Cloud.SDK.Utilities;
using IBM.Cloud.SDK.Authentication.Iam;
using System.Collections;
using UnityEngine;
using IBM.Cloud.SDK;

public class TextToSpeechController : MonoBehaviour
{
    private string serviceUrl = "https://api.us-south.text-to-speech.watson.cloud.ibm.com/instances/b094ba6f-2545-4d17-adb2-6b7fbc3cdfdf";
    private string iamApikey = "vbGjQ6Ce-TVXuktLdwAh3_cPtvW6lGI8lSJ4hN0jAgDN";
    private TextToSpeechService service;
    private string allisionVoice = "pt-BR_IsabelaV3Voice";
    private string synthesizeMimeType = "audio/wav";

    private void Start()
    {
        LogSystem.InstallDefaultReactors();
        Runnable.Run(CreateService());
    }

    public void PlayOnClick(string m_textToSay)
    {
        //Paul, just call this line wherever you want.
        Runnable.Run(ExampleSynthesize(m_textToSay));
    }

    IEnumerator CreateService()
    {
        if (string.IsNullOrEmpty(iamApikey))
        {
            throw new IBMException("Please add IAM ApiKey to the Iam Apikey field in the inspector.");
        }

        IamAuthenticator authenticator = new IamAuthenticator(apikey: iamApikey);

        while (!authenticator.CanAuthenticate())
        {
            yield return null;
        }

        service = new TextToSpeechService(authenticator);
        if (!string.IsNullOrEmpty(serviceUrl))
        {
            service.SetServiceUrl(serviceUrl);
        }
    }

    private IEnumerator ExampleSynthesize(string m_textToSay)
    {
        byte[] synthesizeResponse = null;
        AudioClip clip = null;
        service.Synthesize(
            callback: (DetailedResponse<byte[]> response, IBMError error) =>
            {
                synthesizeResponse = response.Result;
                Log.Debug("ExampleTextToSpeechV1", "Synthesize done!");
                clip = WaveFile.ParseWAV("myClip", synthesizeResponse);
                PlayClip(clip);
            },
            text: m_textToSay,
            voice: allisionVoice,
            accept: synthesizeMimeType
        );

        while (synthesizeResponse == null)
            yield return null;

        yield return new WaitForSeconds(clip.length);
    }

    void PlayClip(AudioClip clip)
    {
        if (Application.isPlaying && clip != null)
        {
            GameObject audioObject = new GameObject("AudioObject");
            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.spatialBlend = 0.0f;
            source.loop = false;
            source.clip = clip;
            source.Play();

            GameObject.Destroy(audioObject, clip.length);
        }
    }

}
