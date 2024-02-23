public static class Constants {
    // Animation - game specific animation constants
    public const float AnimPlaceShapeDur = 0.2f;           // anim duration of shape placement to grids
    public const float AnimDestroyShapeDur = 0.2f;         // anim duration of shape destroy
    public const float AnimIndividualDeliveryDelay = 0.1f; // delay between delivery of individual products
    public const float AnimOrderBubbleFadeDur = 0.75f;     // anim duration of order bubble fade in/out
    public const float AnimDragSnapDur = 0.15f;            // anim duration of moving dragged shapes on grid
    public static readonly DOTweenShakeArgs AnimInvalidShake = new() {
        Duration = 0.2f,
        Strength = 0.1f,
        Vibrato = 30,
        Randomness = 20f
    };
}