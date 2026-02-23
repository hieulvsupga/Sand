using UnityEngine;

public class SandFillController : MonoBehaviour
{
    [SerializeField] private Material sandMaterial;
    [SerializeField] private float fillSpeed = 0.2f;
    [SerializeField] private float velocityRiseSpeed = 10f;  // lên nhanh
    [SerializeField] private float velocityFallSpeed = 2f;   // về 0 chậm → chuyển mượt

    private float _fillAmount;
    private float _fillVelocity;
    private bool _isFilling;

    void Update()
    {
        _isFilling = Input.GetKey(KeyCode.Space);

        if (_isFilling)
        {
            _fillAmount = Mathf.Clamp01(_fillAmount + fillSpeed * Time.deltaTime);
            _fillVelocity = Mathf.MoveTowards(_fillVelocity, 1f, Time.deltaTime * velocityRiseSpeed);
        }
        else
        {
            // Lerp thay vì MoveTowards → giảm chậm dần tự nhiên hơn
            _fillVelocity = Mathf.Lerp(_fillVelocity, 0f, Time.deltaTime * velocityFallSpeed);
        }

        sandMaterial.SetFloat("_FillAmount", _fillAmount);
        sandMaterial.SetFloat("_FillVelocity", _fillVelocity);
    }
}