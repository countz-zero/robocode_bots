using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Erlang : Bot
{
    private int orbitDirection = 1;

    static void Main(string[] args)
    {
        new Erlang().Start();
    }

    Erlang() : base(BotInfo.FromFile("Erlang.json")) { }

    public override void Run()
    {
        // White-grey color theme
        BodyColor = Color.FromArgb(0xF0, 0xF0, 0xF0);    // Off-white
        TurretColor = Color.FromArgb(0x80, 0x80, 0x80);  // Medium grey
        RadarColor = Color.FromArgb(0x40, 0x40, 0x40);   // Dark grey
        BulletColor = Color.FromArgb(0x20, 0x20, 0x20);  // Very dark grey
        ScanColor = Color.FromArgb(0xC0, 0xC0, 0xC0);    // Silver

        SetTurnRadarRight(Double.PositiveInfinity);

        while (IsRunning)
        {
            Forward(50);
            TurnRight(10 * orbitDirection);
            WaitFor(new TickCondition(10));
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {   double BodyHeading = Direction;
        double GunHeading = GunDirection;
        if (IsTeammate(e.ScannedBotId)) return;

        var bearing = BearingTo(e.X, e.Y);
        var distance = DistanceTo(e.X, e.Y);
        if (distance < 200)
        {
            orbitDirection *= -1;
            Back(100);
        }

        var absoluteBearing = BodyHeading + bearing;
        var gunAdjust = NormalizeBearing(absoluteBearing - GunHeading);
        
        SetTurnGunRight(gunAdjust);

        if (Math.Abs(gunAdjust) < 3 && e.Energy > 30)
        {
            Fire(2.5);
        }

        SetTurnRight(NormalizeBearing(bearing + 90 * orbitDirection));
    }

    public override void OnHitBot(HitBotEvent e)
    {
        Back(200);
        TurnLeft(45);
        orbitDirection *= -1;
    }

    private double NormalizeBearing(double bearing)
    {
        return (bearing + 360) % 360;
    }
}

public class TickCondition : Condition
{
    private int _ticks;
    private readonly int _targetTicks;

    public TickCondition(int targetTicks) => _targetTicks = targetTicks;
    public override bool Test() => ++_ticks >= _targetTicks;
}