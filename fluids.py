from noise import pnoise2, pnoise3, snoise3
import matplotlib.pyplot as plt
import numpy as np
import math
import time


def generate_perlin_3d(size, frequency=50, base_value=0, value_offset=1):
    volume = np.zeros((size, size, size))
    m = 0
    for x in range(size):
        for y in range(size):
            for z in range(size):
                v = base_value + pnoise3(x / frequency, y / frequency, z / frequency) * value_offset
                m = v if v > m else m
                volume[x, y, z] = v
    return volume, m


def visualize_slice(grid):
    fig, ax = plt.subplots()
    im = ax.imshow(grid, cmap="gray", vmin=992, vmax=1002)
    fig.colorbar(im)
    plt.show()


def save_slice(grid, path):
    fig, ax = plt.subplots()
    im = ax.imshow(grid, cmap="gray", vmin=992, vmax=1002)
    fig.colorbar(im)
    plt.savefig(path)


def resolve_edges3(x):
    m, n, o = x.shape
    for j in range(1, n - 1, 1):
        for i in range(1, m - 1, 1):
            x[i, j, 0] = x[i, j, 1]
            x[i, j, o - 1] = x[i, j, o - 2]

    for k in range(1, o - 1, 1):
        for i in range(1, m - 1, 1):
            x[i, 0, k] = x[i, 1, k]
            x[i, n - 1, k] = x[i, n - 2, k]

    for k in range(1, o - 1, 1):
        for j in range(1, n - 1, 1):
            x[0, j, k] = x[1, j, k]
            x[m - 1, j, k] = x[m - 2, j, k]


def diffuse3(x, x0, diff, dt):
    m, n, o = x.shape
    a = dt * diff * ((len(x) - 2) ** 3)

    for _ in range(15):
        for i in range(1, m - 1):
            for j in range(1, n - 1):
                for k in range(1, o - 1):
                    x[i, j, k] = (x0[i, j, k] + a * (x[i - 1, j, k] + x[i + 1, j, k] + x[i, j - 1, k] + x[i, j + 1, k] +
                                                     x[i, j, k - 1] + x[i, j, k + 1])) / (1 + 6 * a)
        print("iter")
        resolve_edges3(x)


def add_floor(volume, value):
    volume[:, 0, :] = value


def evaluate_wave(x, y, frequency, amplitude, direction):
    direction /= np.linalg.norm(direction)
    return amplitude * math.sin(frequency * (direction[0] * x + direction[1] * y))


def create_waves(plane, frequency, amplitude, direction):
    m, n = plane.shape
    for i in range(m):
        for j in range(n):
            plane[i, j] += int(round(evaluate_wave(i, j, frequency, amplitude, direction)))
            #plane[i, j] += evaluate_wave(i, j, 0.17, 0.8, [1, 0.2])


def apply_waves(cube, waves, value):
    x, y, z = cube.shape
    m, n = waves.shape
    max_amplitude = np.amax(waves)
    surface_height = y - 1 - max_amplitude
    if m != x and n != z:
        raise ValueError("cube x:z and waves m:n don't match")
    for i in range(m):
        for j in range(n):
            v = int(surface_height + waves[i, j])
            cube[i, v:, j] = value


def export_file(cube, filename):
    x, y, z = cube.shape
    b = bytearray(x*y*z)
    for k in range(z):
        for j in range(y):
            for i in range(x):
                b[i + y * (j + z * k)] = int(cube[i, j, k])
    with open(filename, 'wb') as f:
        f.write(b)


# kg/m3
counter = 0
folder = "C:/Users/Mareee/Desktop/wd"

water_density = 128
noise_offset = 10
size = 128

cube, m = generate_perlin_3d(size, frequency=15, base_value=water_density, value_offset=noise_offset)
print(m)
save_slice(cube[10, :, :], f"{folder}/{counter}.png")
counter += 1
for _ in range(3):
    t1 = time.perf_counter()
    cube0 = cube.copy()
    diffuse3(cube, cube0, 5, 1)
    t2 = time.perf_counter()
    print(f"Diffused: {t2 - t1}s")
    # visualize_slice(cube[:, :, 10])
    save_slice(cube[10, :, :], f"{folder}/{counter}.png")
    counter += 1

add_floor(cube, 255)
wave_map = np.zeros((size, size))
create_waves(wave_map, 0.05, 1.5, [0.1, 1])
create_waves(wave_map, 0.15, 2, [-0.1, 1])
create_waves(wave_map, 0.08, 1.2, [1, 1])
apply_waves(cube, wave_map, 0)
plt.imshow(cube[10, :, :], cmap="gray")
plt.show()
export_file(cube, "output")