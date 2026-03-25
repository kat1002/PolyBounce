using UnityEngine;

public class Ball : MonoBehaviour, IPoolable
{
    [SerializeField] private Rigidbody2D _rigidbody2D;
    private Vector3 _lastVelocity;

    private void Update()
    {
        _lastVelocity = _rigidbody2D.linearVelocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject collObject = collision.gameObject;

        if (collObject.CompareTag("Bottom"))
        {
            //ObjectPool.EnqueueObject(this, "Ball");
            //return;
        }

        if (collObject.CompareTag("Obstacle"))
        {
            //Obstacle obs = collObject.GetComponent<Obstacle>();
            //obs.takeDamage();
        }

        var speed = _lastVelocity.magnitude;
        var direction = Vector3.Reflect(_lastVelocity.normalized, collision.contacts[0].normal);
        _rigidbody2D.linearVelocity = direction * Mathf.Max(speed, 0f);

        //rb.AddForce(direction * speed);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //GameObject collObject = collision.gameObject;
        //if (collObject.CompareTag("Plus"))
        //{
        //    PlusBall pb = collObject.GetComponent<PlusBall>();
        //    pb.AddBall();
        //}
    }

    public void OnSpawn()
    {
        _rigidbody2D.linearVelocity = Vector2.zero;
        _lastVelocity = Vector2.zero;
    }

    public void OnDespawn()
    {
        throw new System.NotImplementedException();
    }
}
