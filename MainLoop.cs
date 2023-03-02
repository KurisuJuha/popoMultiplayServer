using System;

namespace JuhaKurisu.PopoTools.Multiplay.Server;

public class MainLoop
{
    public readonly int fps;
    private readonly Action action;
    private readonly int sleepSpan;

    public MainLoop(int fps, Action action)
    {
        this.fps = fps;
        this.action = action;
        sleepSpan = 1000 / fps;
    }

    public void Start()
    {
        while (true)
        {
            DateTime frameStartTime = DateTime.Now;
            action.Invoke();
            // フレーム終了時間まで待機
            while (sleepSpan > (DateTime.Now - frameStartTime).TotalMilliseconds) { }
        }
    }
}

