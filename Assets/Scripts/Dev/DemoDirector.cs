using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

/// <summary>
/// Plays a level from a plain-text script, for trailer capture and smoke tests.
/// One command per line (# starts a comment):
///
///   rec &lt;folder&gt;          start recording frames (FrameRecorder) into folder
///   stop                   stop recording
///   place x,y              put the kid at a position (to cut between rooms)
///   walk x                 walk until the kid reaches x
///   jump                   jump once
///   interact               press the interact key (elevators)
///   wait s                 wait seconds
///   point x,y [s]          move the cursor to a world position over s seconds
///   drag x,y -> x,y [s]    move the cursor to a box, grab it, drag it, release
///   hold x,y -> x,y [s]    same as drag, but keep holding (release with 'release')
///   release                release the pointer
///   mark name              write a marker to events.txt (for the editor)
///   fly x,y,size -> x,y,size s   take the camera off Cinemachine and glide it (level fly-over)
///   follow                 hand the camera back to Cinemachine
///
/// Every sound effect played and every marker is written to events.txt with its
/// frame number, so the edit can place SFX and captions on the right frame.
/// </summary>
public class DemoDirector : MonoBehaviour {
    public string scriptPath;
    public bool quitWhenDone;

    private ScriptedInput input;
    private FrameRecorder recorder;
    private Transform player;
    private StreamWriter events;
    public bool Done { get; private set; }

    private void Start() {
        input = gameObject.AddComponent<ScriptedInput>();
        recorder = gameObject.AddComponent<FrameRecorder>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        input.pointerWorld = player.position + new Vector3(2f, 2f, 0f);
        Sfx.Played += OnSfx;
        StartCoroutine(Run(File.ReadAllLines(scriptPath)));
    }

    private void OnDestroy() {
        Sfx.Played -= OnSfx;
        events?.Dispose();
    }

    private void OnSfx(string name) => Log("sfx " + name);

    private void Log(string what) {
        if (events != null && recorder.recording) {
            events.WriteLine($"{recorder.FrameCount} {what}");
            events.Flush();
        }
    }

    private static float F(string s) => float.Parse(s, CultureInfo.InvariantCulture);

    private static Vector2 V(string s) {
        var p = s.Split(',');
        return new Vector2(F(p[0]), F(p[1]));
    }

    private IEnumerator Run(IEnumerable<string> lines) {
        yield return null;
        foreach (var raw in lines) {
            var line = raw.Split('#')[0].Trim();
            if (line.Length == 0) {
                continue;
            }
            var a = line.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            switch (a[0]) {
                case "rec":
                    recorder.Begin(a[1]);
                    events = new StreamWriter(Path.Combine(a[1], "events.txt"));
                    break;
                case "stop":
                    recorder.End();
                    break;
                case "place":
                    var pos = V(a[1]);
                    player.position = new Vector3(pos.x, pos.y, player.position.z);
                    var rb = player.GetComponent<Rigidbody2D>();
                    if (rb != null) rb.linearVelocity = Vector2.zero;
                    yield return null;
                    break;
                case "walk":
                    yield return Walk(F(a[1]));
                    break;
                case "jump":
                    input.Jump();
                    yield return null;
                    break;
                case "interact":
                    input.Interact();
                    yield return null;
                    break;
                case "wait":
                    yield return new WaitForSeconds(F(a[1]));
                    break;
                case "point":
                    yield return MovePointer(V(a[1]), a.Length > 2 ? F(a[2]) : 0.4f);
                    break;
                case "drag":
                case "hold":
                    yield return MovePointer(V(a[1]), 0.35f);
                    input.pointerHeld = true;
                    yield return new WaitForSeconds(0.15f);
                    yield return MovePointer(V(a[3]), a.Length > 4 ? F(a[4]) : 0.9f);
                    if (a[0] == "drag") {
                        yield return new WaitForSeconds(0.1f);
                        input.pointerHeld = false;
                    }
                    break;
                case "release":
                    input.pointerHeld = false;
                    yield return null;
                    break;
                case "fly":
                    yield return Fly(a[1], a[3], a.Length > 4 ? F(a[4]) : 4f);
                    break;
                case "follow":
                    SetBrain(true);
                    break;
                case "mark":
                    Log("mark " + string.Join(" ", a, 1, a.Length - 1));
                    break;
            }
        }
        recorder.End();
        events?.Dispose();
        events = null;
        Done = true;
        if (quitWhenDone) {
            Application.Quit();
        }
    }

    private void SetBrain(bool on) {
        var cam = Camera.main;
        if (cam == null) return;
        foreach (var b in cam.GetComponents<Behaviour>()) {
            if (b.GetType().Name == "CinemachineBrain") b.enabled = on;
        }
    }

    private IEnumerator Fly(string from, string to, float seconds) {
        var cam = Camera.main;
        SetBrain(false);
        var f = from.Split(',');
        var t2 = to.Split(',');
        Vector3 p0 = new Vector3(F(f[0]), F(f[1]), cam.transform.position.z);
        Vector3 p1 = new Vector3(F(t2[0]), F(t2[1]), cam.transform.position.z);
        float s0 = F(f[2]), s1 = F(t2[2]);
        float t = 0f;
        while (t < 1f) {
            t = Mathf.Min(1f, t + Time.deltaTime / seconds);
            float e = t * t * (3f - 2f * t);
            cam.transform.position = Vector3.Lerp(p0, p1, e);
            cam.orthographicSize = Mathf.Lerp(s0, s1, e);
            yield return null;
        }
    }

    private IEnumerator Walk(float x) {
        float dir = Mathf.Sign(x - player.position.x);
        float timeout = 12f;
        input.moveX = dir;
        while ((x - player.position.x) * dir > 0.05f && timeout > 0f) {
            timeout -= Time.deltaTime;
            yield return null;
        }
        input.moveX = 0f;
    }

    private IEnumerator MovePointer(Vector2 to, float seconds) {
        Vector2 from = input.pointerWorld;
        float t = 0f;
        while (t < 1f) {
            t = seconds <= 0f ? 1f : Mathf.Min(1f, t + Time.deltaTime / seconds);
            float e = t * t * (3f - 2f * t);
            input.pointerWorld = Vector2.Lerp(from, to, e);
            yield return null;
        }
    }
}
