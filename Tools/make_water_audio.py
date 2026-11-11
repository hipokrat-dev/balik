"""Reproducible, locally synthesized water effects; no speech or external samples."""
from pathlib import Path
import numpy as np
import wave
out = Path(__file__).resolve().parents[1] / 'Assets/Resources/Audio'
rng=np.random.default_rng(2718); rate=44100

def noise(n, low, high):
    f=np.fft.rfftfreq(n, 1/rate)
    shaping=(1-np.exp(-(f/max(low,1))**2))*np.exp(-(f/high)**2)
    a=np.fft.irfft(np.fft.rfft(rng.normal(size=n))*shaping,n)
    return a/max(np.std(a),1e-8)

def save(name,a):
    a=np.clip(a,-.95,.95)
    with wave.open(str(out/(name+'.wav')),'wb') as w:
        w.setnchannels(1);w.setsampwidth(2);w.setframerate(rate);w.writeframes((a*32767).astype('<i2').tobytes())

n=rate*24;t=np.arange(n)/rate
# Periodic envelopes and FFT-filtered noise provide a seamless wave bed.
swell=(.5+.5*np.sin(2*np.pi*t/8))**2
waves=noise(n,70,1400)*(.035+.07*swell)+noise(n,800,5500)*(.006+.016*swell)
save('waves',waves)
n=int(rate*.65);t=np.arange(n)/rate
env=(1-np.exp(-t*100))*np.exp(-t*7)
a=noise(n,250,6500)*env*.13
for onset,f in [(.02,700),(.09,1050),(.17,850)]:
    u=np.maximum(t-onset,0);a+=np.sin(2*np.pi*(f*u-300*u*u))*np.exp(-u*45)*(t>=onset)*.12
save('water-catch',a)
