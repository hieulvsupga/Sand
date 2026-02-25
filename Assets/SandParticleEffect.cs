using UnityEngine;

/// <summary>
/// Tạo hiệu ứng particle cát đổ từ GP_Machine xuống ThreeShape.
/// Cát đổ theo đường cong parabolic tự nhiên như vòi nước.
/// Sử dụng startSpeed + hướng phát + gravity để tạo quỹ đạo chính xác.
/// </summary>
public class SandParticleEffect : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Transform của GP_Machine (nguồn đổ cát)")]
    [SerializeField] private Transform gpMachine;

    [Tooltip("Transform của ThreeShape (nơi nhận cát)")]
    [SerializeField] private Transform threeShape;

    [Header("Particle Settings")]
    [SerializeField] private Color sandColor = new Color(0.85f, 0.72f, 0.35f, 1f);
    [SerializeField] private Color sandColorVariation = new Color(0.95f, 0.82f, 0.45f, 1f);
    [SerializeField] private float emissionRate = 120f;
    [SerializeField] private float particleLifetime = 2f;
    [SerializeField] private float particleSize = 0.06f;

    [Header("Arc Settings (Đường cong vòi nước)")]
    [Tooltip("Tốc độ phát ban đầu của hạt")]
    [SerializeField] private float startSpeed = 2.5f;
    [Tooltip("Góc phát (độ) so với phương ngang. 0=ngang, 90=thẳng xuống")]
    [SerializeField] private float launchAngle = 30f;
    [Tooltip("Hệ số trọng lực (1 = bình thường, >1 = rơi nhanh hơn)")]
    [SerializeField] private float gravity = 1.2f;
    [Tooltip("Góc cone spread (độ rộng dòng cát)")]
    [SerializeField] private float coneAngle = 3f;

    [Header("Spawn Offset")]
    [Tooltip("Offset từ GP_Machine để xác định điểm phát particle (miệng đổ cát)")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(-0.5f, -0.3f, 0f);

    [Header("Direction")]
    [Tooltip("Hướng phát cát trong world space (sẽ được normalize)")]
    [SerializeField] private Vector3 launchDirection = new Vector3(-1f, 0f, 0f);

    private ParticleSystem _particleSystem;
    private ParticleSystem.EmissionModule _emission;
    private Material _particleMaterial;
    private bool _isPouring;

    void Start()
    {
        CreateParticleMaterial();
        SetupParticleSystem();
    }

    void Update()
    {
        _isPouring = Input.GetKey(KeyCode.Space);
        _emission.enabled = _isPouring;

        // Cập nhật vị trí particle theo GP_Machine
        if (gpMachine != null)
        {
            transform.position = gpMachine.position + spawnOffset;
        }
    }

    private void CreateParticleMaterial()
    {
        // Tìm shader phù hợp cho particle
        Shader particleShader = Shader.Find("Particles/Standard Unlit");
        if (particleShader == null)
            particleShader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
        if (particleShader == null)
            particleShader = Shader.Find("Unlit/Color");

        _particleMaterial = new Material(particleShader);
        _particleMaterial.name = "SandParticleMat";
        _particleMaterial.color = sandColor;

        if (_particleMaterial.shader.name == "Particles/Standard Unlit")
        {
            _particleMaterial.SetColor("_Color", sandColor);
        }
    }

    private void SetupParticleSystem()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        if (_particleSystem == null)
        {
            _particleSystem = gameObject.AddComponent<ParticleSystem>();
        }

        _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // ============================================
        // MAIN MODULE
        // ============================================
        var main = _particleSystem.main;
        main.duration = 10f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(particleLifetime * 0.7f, particleLifetime);
        // Tốc độ ban đầu — đây là yếu tố chính tạo đường cong
        main.startSpeed = new ParticleSystem.MinMaxCurve(startSpeed * 0.9f, startSpeed);
        main.startSize = new ParticleSystem.MinMaxCurve(particleSize * 0.5f, particleSize);
        main.startColor = new ParticleSystem.MinMaxGradient(sandColor, sandColorVariation);
        // GRAVITY — kết hợp với startSpeed tạo quỹ đạo parabolic tự nhiên
        main.gravityModifier = gravity;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 1000;
        main.playOnAwake = false;

        // ============================================
        // EMISSION MODULE
        // ============================================
        _emission = _particleSystem.emission;
        _emission.rateOverTime = emissionRate;
        _emission.enabled = false;

        // ============================================
        // SHAPE MODULE — Hướng phát tạo đường cong
        // ============================================
        // Dùng Cone với góc nhỏ, xoay để hướng sang ngang-xuống
        var shape = _particleSystem.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = coneAngle; // Góc cone nhỏ = dòng cát tập trung
        shape.radius = 0.03f;    // Bán kính nhỏ = miệng đổ nhỏ
        shape.radiusThickness = 1f;
        shape.length = 0.01f;

        // Tính rotation để hướng cone theo launchDirection + launchAngle
        // launchDirection mặc định (-1,0,0) = sang trái
        // launchAngle = góc nghiêng xuống so với phương ngang
        Vector3 dir = launchDirection.normalized;
        // Xoay hướng xuống theo launchAngle
        float angleRad = launchAngle * Mathf.Deg2Rad;
        Vector3 finalDir = new Vector3(
            dir.x * Mathf.Cos(angleRad),
            -Mathf.Sin(angleRad),
            dir.z
        ).normalized;

        // Tính rotation từ Vector3.forward (hướng mặc định của cone) sang finalDir
        Quaternion rot = Quaternion.FromToRotation(Vector3.forward, finalDir);
        shape.rotation = rot.eulerAngles;

        // ============================================
        // TẮT Velocity over Lifetime — để gravity tự tạo đường cong
        // ============================================
        var velocity = _particleSystem.velocityOverLifetime;
        velocity.enabled = false;

        // ============================================
        // COLOR OVER LIFETIME
        // ============================================
        var colorOverLifetime = _particleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(sandColor, 0f),
                new GradientColorKey(sandColor, 0.7f),
                new GradientColorKey(sandColor * 0.8f, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        // ============================================
        // SIZE OVER LIFETIME
        // ============================================
        var sizeOverLifetime = _particleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.8f);
        sizeCurve.AddKey(0.3f, 1f);
        sizeCurve.AddKey(0.7f, 0.7f);
        sizeCurve.AddKey(1f, 0.2f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // ============================================
        // NOISE — nhiễu nhẹ cho tự nhiên
        // ============================================
        var noise = _particleSystem.noise;
        noise.enabled = true;
        noise.strength = 0.1f;
        noise.frequency = 1.5f;
        noise.scrollSpeed = 0.3f;
        noise.damping = true;
        noise.octaveCount = 2;

        // ============================================
        // COLLISION — va chạm với ThreeShape
        // ============================================
        var collision = _particleSystem.collision;
        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World;
        collision.mode = ParticleSystemCollisionMode.Collision3D;
        collision.bounce = 0.02f;
        collision.dampen = 0.9f;
        collision.lifetimeLoss = 0.5f;
        collision.enableDynamicColliders = true;

        // ============================================
        // RENDERER — gán material
        // ============================================
        var renderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingOrder = 5;
        if (_particleMaterial != null)
        {
            renderer.material = _particleMaterial;
        }

        // Vị trí ban đầu
        if (gpMachine != null)
        {
            transform.position = gpMachine.position + spawnOffset;
        }

        _particleSystem.Play();
    }

    /// <summary>
    /// Cho phép bật/tắt đổ cát từ script khác
    /// </summary>
    public void SetPouring(bool pouring)
    {
        _isPouring = pouring;
        _emission.enabled = _isPouring;
    }

    /// <summary>
    /// Cập nhật emission rate runtime
    /// </summary>
    public void SetEmissionRate(float rate)
    {
        emissionRate = rate;
        _emission.rateOverTime = rate;
    }

    void OnDestroy()
    {
        if (_particleMaterial != null)
        {
            Destroy(_particleMaterial);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Vector3 spawnPos = gpMachine != null
            ? gpMachine.position + spawnOffset
            : transform.position;

        Gizmos.color = new Color(0.85f, 0.72f, 0.35f, 0.8f);
        Gizmos.DrawWireSphere(spawnPos, 0.1f);

        // Vẽ đường cong parabolic preview — ĐÚNG với physics thực tế
        // v0 = startSpeed theo hướng (launchDirection rotated by launchAngle)
        Vector3 dir = launchDirection.normalized;
        float angleRad = launchAngle * Mathf.Deg2Rad;
        Vector3 v0 = new Vector3(
            dir.x * Mathf.Cos(angleRad),
            -Mathf.Sin(angleRad),
            dir.z
        ).normalized * startSpeed;

        Vector3 pos = spawnPos;
        float dt = 0.02f;
        float g = gravity * Physics.gravity.y; // gravity.y thường = -9.81

        for (int i = 0; i < 80; i++)
        {
            float t = i * dt;
            // Vị trí parabolic: p = p0 + v0*t + 0.5*g*t^2
            Vector3 nextPos = spawnPos + v0 * (t + dt) + new Vector3(0, 0.5f * g * (t + dt) * (t + dt), 0);
            Vector3 curPos = spawnPos + v0 * t + new Vector3(0, 0.5f * g * t * t, 0);

            float alpha = 1f - (float)i / 80f;
            Gizmos.color = new Color(0.85f, 0.72f, 0.35f, alpha);
            Gizmos.DrawLine(curPos, nextPos);

            // Dừng nếu đã rơi quá xa
            if (nextPos.y < spawnPos.y - 5f) break;
        }

        if (threeShape != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(threeShape.position, Vector3.one * 0.5f);
        }
    }
#endif
}
