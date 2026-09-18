# Target Visual / Gameplay Direction

High-speed night driving, cinematic chase camera, wet reflective PBR road, HDRI environment lighting, practical headlights, roadside light pools, sparse escalating infected, physical impact/tumble, jumps and vehicle reinforcement upgrades.

This is not a clone of Need for Speed Rivals. No proprietary EA/Ghost assets, branding, maps, vehicles, audio or code are included.

## Production layers
1. Core driving + gamepad + camera — implemented baseline.
2. Procedural road + encounter spacing — implemented.
3. Zombie hit physics + escalating strength — implemented baseline.
4. HDRP night grade + HDRI/PBR bootstrap — implemented.
5. Modern CC0 car/infected/environment asset bootstrap — implemented.
6. Next visual value: rain/water spray, tire smoke, collision VFX, layered engine/audio and vegetation.
7. Final production: LODs, occlusion, GPU/CPU profiling, animation polish and Windows quality presets.

## Cost/complexity guard
Do not build custom rendering or asset pipelines while high-quality CC0 assets and standard HDRP features cover the need. Prefer visible frame quality and stable Windows builds over architectural complexity.
