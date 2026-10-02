"""Regenerate Subsistence's original procedural WAV palette (Python standard library only)."""
import math
import random
import struct
import wave
from pathlib import Path

SAMPLE_RATE = 22050
OUT = Path(__file__).parents[1] / "Assets" / "Resources" / "Audio"
OUT.mkdir(parents=True, exist_ok=True)
RNG = random.Random(404)


def save(name, samples):
    peak = max((abs(value) for value in samples), default=1.0)
    scale = min(.82, .72 / max(peak, 1e-9))
    with wave.open(str(OUT / (name + ".wav")), "wb") as file:
        file.setnchannels(1)
        file.setsampwidth(2)
        file.setframerate(SAMPLE_RATE)
        file.writeframes(b"".join(struct.pack("<h", int(max(-1, min(1, s * scale)) * 32767)) for s in samples))


def envelope(t, duration, attack=.01, release=.08):
    return min(1, t / max(attack, 1e-5)) * min(1, max(0, duration-t) / max(release, 1e-5))


def ambience():
    duration = 9.0
    count = int(SAMPLE_RATE * duration)
    filtered_noise = []
    lowpass = 0.0
    for _ in range(count):
        lowpass = lowpass * .994 + RNG.uniform(-1, 1) * .006
        filtered_noise.append(lowpass)
    data = []
    for i in range(count):
        t = i / SAMPLE_RATE
        drone = .22*math.sin(2*math.pi*48*t) + .13*math.sin(2*math.pi*72.4*t+.4) + .07*math.sin(2*math.pi*96.2*t+1.1)
        distant = .035*math.sin(2*math.pi*(193+2*math.sin(t*.25))*t)
        data.append((drone+distant+filtered_noise[i]*.5)*(.78+.06*math.sin(t*.47)))
    fade = int(SAMPLE_RATE*.22)
    for i in range(fade):
        q = i/fade
        blend = data[-fade+i]*(1-q)+data[i]*q
        data[i] = data[-fade+i] = blend
    save("ambience_service_tunnel", data)


def fluorescent_hum():
    """Loopable, gently unstable mains buzz for the empty Level 0 ceiling lights."""
    duration = 8.0
    count = int(SAMPLE_RATE * duration)
    rng = random.Random(601)
    noise = 0.0
    data = []
    for i in range(count):
        t = i / SAMPLE_RATE
        noise = noise * .965 + rng.uniform(-1, 1) * .035
        drift = .86 + .09 * math.sin(2 * math.pi * .17 * t) + .05 * math.sin(2 * math.pi * .43 * t + .7)
        mains = (.19 * math.sin(2 * math.pi * 60 * t) +
                 .24 * math.sin(2 * math.pi * 120 * t + .2) +
                 .11 * math.sin(2 * math.pi * 240 * t + .6) +
                 .045 * math.sin(2 * math.pi * 960 * t + 1.2))
        data.append(drift * (mains + noise * .16))
    fade = int(SAMPLE_RATE * .20)
    for i in range(fade):
        q = i / fade
        blend = data[-fade + i] * (1 - q) + data[i] * q
        data[i] = data[-fade + i] = blend
    save("fluorescent_level0", data)


def score():
    duration = 24.0
    rng = random.Random(92)
    notes = [(220,0,5.5),(261.63,5.5,4.2),(196,9.7,5.4),(174.61,15.1,4.0),(146.83,19.1,4.9)]
    data = []
    noise = 0.0
    for i in range(int(SAMPLE_RATE*duration)):
        t = i/SAMPLE_RATE
        base = .20*math.sin(2*math.pi*55*t+.25*math.sin(t*.31)) + .10*math.sin(2*math.pi*73.42*t) + .055*math.sin(2*math.pi*110*t)
        lead = 0.0
        for frequency, start, length in notes:
            u = t-start
            if 0 <= u < length:
                swell = min(1,u/1.2)*min(1,(length-u)/1.4)
                lead += swell*(.12*math.sin(2*math.pi*frequency*u)+.035*math.sin(2*math.pi*frequency*2.003*u))
        noise = noise*.994+rng.uniform(-1,1)*.006
        data.append((base+lead+noise*.28)*(.85+.15*math.sin(2*math.pi*t/8)))
    fade = int(SAMPLE_RATE*1.2)
    for i in range(fade):
        a = i/(fade-1)
        blend = data[-fade+i]*math.cos(a*math.pi/2)+data[i]*math.sin(a*math.pi/2)
        data[i] = data[-fade+i] = blend
    save("theme_subsistence", data)


def effects():
    def spec(name, duration, make):
        count = int(duration*SAMPLE_RATE)
        save(name, [make(i/SAMPLE_RATE,duration) for i in range(count)])
    spec("ui_confirm",.24,lambda t,d: math.sin(2*math.pi*(620+140*t/d)*t)*.55*envelope(t,d,.008,.13)+math.sin(2*math.pi*(930+100*t/d)*t)*.22*envelope(t,d,.012,.11))
    spec("pickup_metal",.46,lambda t,d: (math.sin(2*math.pi*1040*t)+.62*math.sin(2*math.pi*1564*t))*math.exp(-t*8)*.42*envelope(t,d,.002,.12)+RNG.uniform(-1,1)*math.exp(-t*42)*.13)
    spec("pickup_water",.43,lambda t,d: math.sin(2*math.pi*(480-220*t/d)*t)*.38*envelope(t,d,.01,.17)+math.sin(2*math.pi*(780-300*t/d)*t)*.20*envelope(t,d,.03,.12)+RNG.uniform(-1,1)*.08*envelope(t,d,.01,.12))
    spec("pickup_cloth",.30,lambda t,d: RNG.uniform(-1,1)*.2*envelope(t,d,.01,.19)+math.sin(2*math.pi*330*t)*.16*envelope(t,d,.01,.12))
    spec("footstep_concrete",.18,lambda t,d: (math.sin(2*math.pi*(88-25*t/d)*t)*.6+RNG.uniform(-1,1)*.23)*envelope(t,d,.003,.12))
    spec("melee_swipe",.23,lambda t,d: (RNG.uniform(-1,1)*.6+math.sin(2*math.pi*(240-130*t/d)*t)*.24)*envelope(t,d,.012,.15))
    spec("watcher_hit",.37,lambda t,d: (math.sin(2*math.pi*(82-48*t/d)*t)*.7+RNG.uniform(-1,1)*.25*math.exp(-t*18))*envelope(t,d,.002,.22))
    spec("watcher_stun",.35,lambda t,d: (math.sin(2*math.pi*(440-330*t/d)*t)*.34+math.sin(2*math.pi*810*t)*.16)*envelope(t,d,.002,.21))


if __name__ == "__main__":
    ambience()
    fluorescent_hum()
    score()
    effects()
    print(f"Wrote original WAV assets to {OUT}")
