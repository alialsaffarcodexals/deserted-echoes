// ─────────────────────────────────────────────────────────────
// OpeningCinematicAudio.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Author: Ali Husain Ali Alsaffar (202301152)
// Description: Procedural audio engine for the opening cinematic,
//              ported from the original HTML cinematic's Web Audio
//              code. Generates every sound at runtime (no audio
//              files): wind/fire ambience, footsteps, combat SFX,
//              and crossfading mood pads with rhythmic pulses.
//              Added to the cinematic GameObject by OpeningCinematic.
// ─────────────────────────────────────────────────────────────

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpeningCinematicAudio : MonoBehaviour
{
    private const int SR = 44100;

    private AudioSource _musicA, _musicB, _wind, _fire, _sfx;
    private bool _usingA = true;
    private string _mood = null;

    private AudioClip _footstep, _thud, _whoosh, _shoutLow, _shoutHigh, _beep, _fireBurst, _kick, _heartKick;
    private readonly Dictionary<string, AudioClip> _pads = new Dictionary<string, AudioClip>();

    private Coroutine _footCo, _pulseCo, _windFadeCo, _fireFadeCo, _musicFadeA, _musicFadeB;

    // ── Setup ────────────────────────────────────────────────
    public void Init()
    {
        _musicA = NewSource(0f, true);
        _musicB = NewSource(0f, true);
        _wind   = NewSource(0f, true);
        _fire   = NewSource(0f, true);
        _sfx    = NewSource(1f, false);

        _wind.clip = BuildWind();
        _fire.clip = BuildFire();

        _footstep  = BuildFootstep();
        _thud      = BuildThud();
        _whoosh    = BuildWhoosh();
        _shoutLow  = BuildShout(500f);
        _shoutHigh = BuildShout(680f);
        _beep      = BuildBeep();
        _fireBurst = BuildFireBurst();
        _kick      = BuildKick(95f, 45f);
        _heartKick = BuildKick(75f, 35f);
    }

    private AudioSource NewSource(float vol, bool loop)
    {
        var s = gameObject.AddComponent<AudioSource>();
        s.playOnAwake = false;
        s.loop = loop;
        s.volume = vol;
        s.spatialBlend = 0f;
        return s;
    }

    // ── Ambience: wind ───────────────────────────────────────
    public void StartWind(float vol)
    {
        if (!_wind.isPlaying) _wind.Play();
        SetWindVolume(vol);
    }

    public void SetWindVolume(float vol)
    {
        if (_windFadeCo != null) StopCoroutine(_windFadeCo);
        _windFadeCo = StartCoroutine(Ramp(_wind, vol, 1.2f, false));
    }

    public void StopWind()
    {
        if (_windFadeCo != null) StopCoroutine(_windFadeCo);
        _windFadeCo = StartCoroutine(Ramp(_wind, 0f, 1f, true));
    }

    // ── Ambience: fire ───────────────────────────────────────
    public void SetFire(float vol)
    {
        if (vol <= 0f)
        {
            if (_fireFadeCo != null) StopCoroutine(_fireFadeCo);
            _fireFadeCo = StartCoroutine(Ramp(_fire, 0f, 1f, true));
            return;
        }
        if (!_fire.isPlaying) _fire.Play();
        if (_fireFadeCo != null) StopCoroutine(_fireFadeCo);
        _fireFadeCo = StartCoroutine(Ramp(_fire, vol, 1f, false));
    }

    // ── Footsteps ────────────────────────────────────────────
    public void StartFootsteps(float intervalSec = 0.55f)
    {
        StopFootsteps();
        _footCo = StartCoroutine(FootstepLoop(intervalSec));
    }

    public void StopFootsteps()
    {
        if (_footCo != null) { StopCoroutine(_footCo); _footCo = null; }
    }

    private IEnumerator FootstepLoop(float interval)
    {
        while (true)
        {
            _sfx.PlayOneShot(_footstep, 0.5f);
            yield return new WaitForSeconds(interval + Random.Range(-0.04f, 0.04f));
        }
    }

    // ── One-shots ────────────────────────────────────────────
    public void PlayThud() => _sfx.PlayOneShot(_thud, 1f);
    public void PlayWhoosh(float vol) => _sfx.PlayOneShot(_whoosh, vol);
    public void PlayShout(bool lucian) => _sfx.PlayOneShot(lucian ? _shoutLow : _shoutHigh, 0.7f);
    public void PlayBeep() => _sfx.PlayOneShot(_beep, 0.5f);
    public void PlayFireBurst() => _sfx.PlayOneShot(_fireBurst, 0.7f);

    public void PlayRush()
    {
        StartCoroutine(RushRoutine());
        PlayWhoosh(0.3f);
    }

    private IEnumerator RushRoutine()
    {
        for (int i = 0; i < 14; i++)
        {
            _sfx.PlayOneShot(_footstep, 0.6f + Random.Range(0f, 0.2f));
            yield return new WaitForSeconds(0.09f + Random.Range(0f, 0.03f));
        }
    }

    // ── Music moods (crossfading pads + pulse) ───────────────
    public void SetMusic(string mood)
    {
        if (_mood == mood) return;
        _mood = mood;

        if (_pulseCo != null) { StopCoroutine(_pulseCo); _pulseCo = null; }

        float vol = MoodVolume(mood);
        AudioClip pad = (mood == "silent" || vol <= 0f) ? null : GetPad(mood);

        AudioSource incoming = _usingA ? _musicB : _musicA;
        AudioSource outgoing = _usingA ? _musicA : _musicB;
        _usingA = !_usingA;

        if (_musicFadeA != null) StopCoroutine(_musicFadeA);
        if (_musicFadeB != null) StopCoroutine(_musicFadeB);
        _musicFadeA = StartCoroutine(Ramp(outgoing, 0f, 1.2f, true));

        if (pad != null)
        {
            incoming.clip = pad;
            incoming.volume = 0f;
            incoming.Play();
            _musicFadeB = StartCoroutine(Ramp(incoming, vol, 1.6f, false));
        }

        float pulse = PulseInterval(mood);
        if (pulse > 0f)
            _pulseCo = StartCoroutine(PulseLoop(mood, pulse));
    }

    private IEnumerator PulseLoop(string mood, float interval)
    {
        bool heart = mood == "ominous-cave" || mood == "dark-resolve";
        float vol = heart ? 0.22f : (mood == "combat" ? 0.34f : mood.StartsWith("thrilling") ? 0.34f : 0.3f);
        int beat = 0;
        while (true)
        {
            _sfx.PlayOneShot(heart ? _heartKick : _kick, vol);
            beat++;
            if (heart) yield return new WaitForSeconds(beat % 2 == 1 ? 0.38f : 1.4f);
            else yield return new WaitForSeconds(interval);
        }
    }

    private static float MoodVolume(string m)
    {
        switch (m)
        {
            case "warm": return 0.5f;
            case "warm-soft": return 0.3f;
            case "tense": return 0.5f;
            case "combat": return 0.55f;
            case "mournful": return 0.45f;
            case "determined-low": return 0.35f;
            case "determined-mid": return 0.55f;
            case "determined-full": return 0.7f;
            case "ominous-cave": return 0.5f;
            case "dark-resolve": return 0.5f;
            case "thrilling-reveal": return 0.6f;
            case "thrilling-resolve": return 0.75f;
            default: return 0f;
        }
    }

    private static float PulseInterval(string m)
    {
        switch (m)
        {
            case "tense": return 1.1f;
            case "combat": return 0.5f;
            case "determined-mid":
            case "determined-full": return 1.1f;
            case "thrilling-reveal":
            case "thrilling-resolve": return 0.3f;
            case "ominous-cave":
            case "dark-resolve": return 0.5f;   // handled as heartbeat
            default: return 0f;
        }
    }

    private AudioClip GetPad(string mood)
    {
        if (_pads.TryGetValue(mood, out AudioClip cached)) return cached;
        float[] freqs = MoodFreqs(mood, out bool[] saw);
        AudioClip clip = BuildPad(mood, freqs, saw, 9f);
        _pads[mood] = clip;
        return clip;
    }

    private static float[] MoodFreqs(string m, out bool[] saw)
    {
        float[] f;
        switch (m)
        {
            case "warm":
            case "warm-soft": f = new[] { 110f, 220f, 277.18f, 329.63f, 440f }; break;
            case "tense": f = new[] { 55f, 110f, 220f }; break;
            case "combat": f = new[] { 58f, 116f, 87f, 174f }; break;
            case "mournful": f = new[] { 110f, 220f, 261.63f, 329.63f }; break;
            case "determined-low":
            case "determined-mid":
            case "determined-full": f = new[] { 73.42f, 146.83f, 220f, 293.66f, 369.99f, 440f }; break;
            case "ominous-cave":
            case "dark-resolve": f = new[] { 41.2f, 73.42f, 77.78f, 146.83f }; break;
            case "thrilling-reveal":
            case "thrilling-resolve": f = new[] { 73.42f, 110f, 174.61f, 293.66f }; break;
            default: f = new[] { 220f }; break;
        }
        saw = new bool[f.Length];
        for (int i = 0; i < f.Length; i++) saw[i] = f[i] < 130f;   // low voices use saw
        return f;
    }

    // ── Procedural clip builders ─────────────────────────────
    private static AudioClip MakeClip(string name, float[] data)
    {
        var clip = AudioClip.Create(name, data.Length, 1, SR, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static void Normalize(float[] d, float peak)
    {
        float max = 0f;
        for (int i = 0; i < d.Length; i++) { float a = Mathf.Abs(d[i]); if (a > max) max = a; }
        if (max < 1e-5f) return;
        float g = peak / max;
        for (int i = 0; i < d.Length; i++) d[i] *= g;
    }

    // Blend the tail into the head so a looped clip has no click.
    private static float[] LoopSmooth(float[] d, int cf)
    {
        int m = d.Length - cf;
        if (m <= 0) return d;
        for (int i = 0; i < cf; i++)
        {
            float w = (float)i / cf;
            d[i] = d[i] * w + d[m + i] * (1f - w);
        }
        var outp = new float[m];
        System.Array.Copy(d, outp, m);
        return outp;
    }

    private static float Saw(float phase) => 2f * (phase - Mathf.Floor(phase + 0.5f));

    private AudioClip BuildPad(string name, float[] freqs, bool[] saw, float seconds)
    {
        int n = Mathf.RoundToInt(seconds * SR);
        var d = new float[n];
        for (int v = 0; v < freqs.Length; v++)
        {
            float f = freqs[v];
            float lfoRate = 0.08f + v * 0.03f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SR;
                float detune = 1f + 0.0015f * Mathf.Sin(2f * Mathf.PI * lfoRate * t);
                float ph = f * detune * t;
                float s = saw[v] ? Saw(ph) * 0.6f : Mathf.Sin(2f * Mathf.PI * ph);
                d[i] += s;
            }
        }
        // slow amplitude swell
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            d[i] *= 0.85f + 0.15f * Mathf.Sin(2f * Mathf.PI * 0.06f * t);
        }
        Normalize(d, 0.3f);
        d = LoopSmooth(d, SR / 4);
        return MakeClip(name, d);
    }

    private AudioClip BuildWind()
    {
        int n = 4 * SR;
        var d = new float[n];
        for (int i = 0; i < n; i++) d[i] = Random.value * 2f - 1f;
        OnePoleLP(d, 500f);
        OnePoleHP(d, 80f);
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            d[i] *= 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * 0.15f * t);
        }
        Normalize(d, 0.5f);
        d = LoopSmooth(d, SR / 4);
        return MakeClip("wind", d);
    }

    private AudioClip BuildFire()
    {
        int n = 3 * SR;
        var d = new float[n];
        for (int i = 0; i < n; i++) d[i] = (Random.value * 2f - 1f);
        OnePoleLP(d, 220f);
        for (int i = 0; i < n; i++) d[i] *= 0.45f;
        // crackle pops
        int pops = 220;
        for (int p = 0; p < pops; p++)
        {
            int start = Random.Range(0, n - 4000);
            float amp = 0.3f + Random.value * 0.5f;
            int len = Random.Range(400, 1800);
            for (int i = 0; i < len && start + i < n; i++)
            {
                float env = Mathf.Exp(-i / (len * 0.25f));
                d[start + i] += (Random.value * 2f - 1f) * amp * env;
            }
        }
        Normalize(d, 0.5f);
        d = LoopSmooth(d, SR / 5);
        return MakeClip("fire", d);
    }

    private AudioClip BuildFootstep()
    {
        int n = Mathf.RoundToInt(0.16f * SR);
        var d = new float[n];
        for (int i = 0; i < n; i++) d[i] = Random.value * 2f - 1f;
        OnePoleLP(d, 700f);
        for (int i = 0; i < n; i++) d[i] *= Mathf.Exp(-i / (n * 0.18f));
        Normalize(d, 0.8f);
        return MakeClip("footstep", d);
    }

    private AudioClip BuildThud()
    {
        int n = Mathf.RoundToInt(0.6f * SR);
        var d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            float freq = Mathf.Lerp(220f, 35f, Mathf.Clamp01(t / 0.3f));
            float env = Mathf.Exp(-t / 0.18f);
            d[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env;
        }
        var noise = new float[n];
        for (int i = 0; i < n; i++) noise[i] = (Random.value * 2f - 1f);
        OnePoleLP(noise, 1200f);
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            d[i] += noise[i] * 0.5f * Mathf.Exp(-t / 0.08f);
        }
        Normalize(d, 0.9f);
        return MakeClip("thud", d);
    }

    private AudioClip BuildWhoosh()
    {
        int n = Mathf.RoundToInt(1.2f * SR);
        var d = new float[n];
        for (int i = 0; i < n; i++) d[i] = Random.value * 2f - 1f;
        OnePoleHP(d, 350f);
        OnePoleLP(d, 2400f);
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            float env = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / 1.2f));
            d[i] *= env;
        }
        Normalize(d, 0.7f);
        return MakeClip("whoosh", d);
    }

    private AudioClip BuildShout(float freq)
    {
        int n = Mathf.RoundToInt(0.7f * SR);
        var d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            float f = Mathf.Lerp(freq, freq * 0.7f, Mathf.Clamp01(t / 0.6f));
            float env = Mathf.Exp(-t / 0.25f);
            d[i] = Saw(f * t) * env;
        }
        OnePoleHP(d, 600f);
        OnePoleLP(d, 1800f);
        Normalize(d, 0.6f);
        return MakeClip("shout", d);
    }

    private AudioClip BuildBeep()
    {
        int n = Mathf.RoundToInt(0.06f * SR);
        var d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            float env = Mathf.Exp(-t / 0.02f);
            d[i] = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 770f * t)) * env;
        }
        Normalize(d, 0.5f);
        return MakeClip("beep", d);
    }

    private AudioClip BuildFireBurst()
    {
        int n = SR;
        var d = new float[n];
        for (int i = 0; i < n; i++) d[i] = Random.value * 2f - 1f;
        OnePoleLP(d, 2600f);
        OnePoleHP(d, 300f);
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            d[i] *= Mathf.Exp(-t / 0.35f);
        }
        // low whump
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            float f = Mathf.Lerp(140f, 50f, Mathf.Clamp01(t / 0.4f));
            d[i] += Mathf.Sin(2f * Mathf.PI * f * t) * 0.5f * Mathf.Exp(-t / 0.2f);
        }
        Normalize(d, 0.8f);
        return MakeClip("fireburst", d);
    }

    private AudioClip BuildKick(float fStart, float fEnd)
    {
        int n = Mathf.RoundToInt(0.4f * SR);
        var d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            float f = Mathf.Lerp(fStart, fEnd, Mathf.Clamp01(t / 0.25f));
            d[i] = Mathf.Sin(2f * Mathf.PI * f * t) * Mathf.Exp(-t / 0.12f);
        }
        Normalize(d, 0.9f);
        return MakeClip("kick", d);
    }

    // ── Simple one-pole filters (in place) ───────────────────
    private static void OnePoleLP(float[] d, float fc)
    {
        float dt = 1f / SR;
        float rc = 1f / (2f * Mathf.PI * fc);
        float a = dt / (rc + dt);
        float y = 0f;
        for (int i = 0; i < d.Length; i++) { y += a * (d[i] - y); d[i] = y; }
    }

    private static void OnePoleHP(float[] d, float fc)
    {
        float dt = 1f / SR;
        float rc = 1f / (2f * Mathf.PI * fc);
        float a = rc / (rc + dt);
        float prevIn = d.Length > 0 ? d[0] : 0f;
        float prevOut = 0f;
        for (int i = 0; i < d.Length; i++)
        {
            float x = d[i];
            prevOut = a * (prevOut + x - prevIn);
            prevIn = x;
            d[i] = prevOut;
        }
    }

    private IEnumerator Ramp(AudioSource src, float target, float dur, bool stopAtEnd)
    {
        float from = src.volume;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            src.volume = Mathf.Lerp(from, target, t / dur);
            yield return null;
        }
        src.volume = target;
        if (stopAtEnd && target <= 0f) src.Stop();
    }
}
