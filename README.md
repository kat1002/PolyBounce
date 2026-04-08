# PolyBounce
    A 2D arcade brick-breaking game built in Unity (C#), inspired by BBTAN.
## Features
    - Complete gameplay loop with round-based difficulty scaling (shooting, ball physics, obstacle destruction)
    - Physics-based trajectory preview with multi-bounce reflection for responsive, intuitive player controls
    - Object pooling for balls and obstacles to reduce runtime memory allocation and improve performance
    - State-machine game loop (`StartRound → InRound → PostRound`) with a decoupled event system for clean cross-system communication
    - Procedural obstacle spawning with shuffled grid placement and configurable spawn rates for special entities (extra balls, shooter obstacles)
