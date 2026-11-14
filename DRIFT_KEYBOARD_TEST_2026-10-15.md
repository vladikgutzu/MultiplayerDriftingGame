# Keyboard Play Mode regression — 2026-10-15

Executed in the already-open Unity 6000.4.7f1 editor with the GARIKA player
prefab spawned by DriftTrack. The runner placed the car on an isolated flat
collider at runtime. No scene or prefab was saved and no player was built.

Input path: virtual Input System Keyboard device → actual RCCP action bindings
→ RCCP_InputManager → RCCP_Input, including countersteering, steering curve,
gear-shift throttle cuts and the physical steering axle. This is an automated
device-level test, not a human driving assessment or OS keyboard test.

Sequence: W acceleration, a 150 ms D press with a brief Space press, D held,
immediate D-to-A reversal, A held, then all keys released. Twenty simulated
seconds, with zero raw throttle/steering mismatches during stable windows.

| Measurement | Before yaw damping | After yaw damping |
| --- | ---: | ---: |
| 0–50 km/h | 2.54 s | 2.56 s |
| Peak yaw rate | 114.42 deg/s | 90.14 deg/s |
| Peak absolute reported slip | 45.44 deg | 42.31 deg |
| Mean drifting speed | 42.39 km/h | 48.70 km/h |
| Time reported drifting | 11.14 s | 11.08 s |
| Time reported slip above 60 degrees | 0 s | 0 s |

The 65 deg/s setting is the onset of extra damping, not a hard angular-velocity
limit. Full held steering can still exceed it. At the end of release, wheel
angle was 0.04 degrees and yaw rate was -1.99 deg/s.

Fixed an additional lifecycle defect: RCCP_InputManager previously left all
three Input Action maps enabled when destroyed. The generated input wrapper
reported leak assertions on reload. The manager now disables and disposes its
wrapper on destruction. No corresponding assertions appeared in the clean
final run. The editor returned to Edit Mode and restored its prior scene setup
and the two preferences changed to start this test.

## Outstanding

- Holding full lock for several seconds still scrubs speed down to about
  13 km/h. This test does not certify satisfactory sustained full-lock handling.
- The flat test collider does not validate track materials, bumps or kerbs.
- Fixed initial-speed cases at 30/50/80 km/h, other vehicles, braking/reverse,
  and multiplayer synchronization still require dedicated runs.
- Reported slip is the controller's own value; a separate signed full-range
  velocity/body-angle measurement is preferable for detecting reverse spins.

## Progressive steering follow-up

The final steering change reduces inward road steering progressively from
55 degrees at walking speed to 26 degrees at 30 km/h. Full 55-degree geometry
remains available for catching a developed slide with countersteer. The normal
axle steering slew rate still applies; angular velocity and travel velocity
are not assigned or clamped by the gameplay controller.

In the same digital-keyboard regression, peak yaw decreased to 75.12 deg/s,
mean drifting speed increased to 60.10 km/h, minimum reported drifting speed
increased to 26.29 km/h and drift time increased to 14.08 s. Peak reported slip
was 41.39 degrees, with no input mismatches or reported angles above 60 degrees.
This supersedes the full-lock slowdown finding for that exact test sequence.

Slip measurement in the gameplay controller now uses signed forward velocity,
so backwards travel cannot be misreported as a small forward drift angle.
The expanded test also independently measures the velocity/body travel angle,
minimum sustained-hold speed and enforces physical acceptance bounds.

### Final controlled-entry series

All three runs completed in the live editor on the final code, with ordinary
keyboard processing enabled. At the start of the manoeuvre, velocity was set
once to the named entry speed; the rest of the manoeuvre ran freely. Each case
used full digital steering, a brief handbrake entry, opposite lock and release.

| Entry speed | Minimum sustained-hold speed | Mean drifting speed | Peak travel angle | Peak yaw rate | Verdict |
| --- | ---: | ---: | ---: | ---: | --- |
| 30 km/h | 31.08 km/h | 43.21 km/h | 40.45 deg | 71.67 deg/s | Pass |
| 50 km/h | 38.01 km/h | 51.41 km/h | 40.59 deg | 73.78 deg/s | Pass |
| 80 km/h | 43.76 km/h | 58.72 km/h | 41.55 deg | 75.27 deg/s | Pass |

Acceptance requires zero keyboard mismatches, a detected drift, minimum speed
above 20 km/h during the sustained hold (6–17.5 s), independent travel angle
below 70 degrees, and peak yaw below 100 deg/s. All cases met these bounds.
At the end of key release, residual wheel angles were 0.45/0.43/0.21 degrees;
raw throttle and steer were zero in all three cases. No compile errors,
exceptions or input-wrapper leak assertions appeared in the final run's
completion log. The editor returned to Edit Mode with zero error/warning
counts in its console snapshot.

The full-lock speed regression and fixed-entry-speed checks listed above are
now covered for GARIKA on the flat test pad. Other cars, track-specific
surfaces/bumps and multiplayer are still outside this validation.

Final raw results are in
`Library/BD_DriftAutotest/BD_KeyboardAutotest.json` and `.csv`. Those files are
generated and ignored by Git. The previous analog test bypassed RCCP external
input processing, so its figures should not be treated as keyboard validation.
