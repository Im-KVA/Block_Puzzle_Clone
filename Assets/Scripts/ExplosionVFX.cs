using UnityEngine;
using DG.Tweening;

public class ExplosionVFX : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particleSystem;

    public void Setup(Sprite[] explosionSprites)
    {
        if (_particleSystem == null)
            _particleSystem = GetComponent<ParticleSystem>();
        var tsa = _particleSystem.textureSheetAnimation;
        tsa.mode = ParticleSystemAnimationMode.Sprites;

        tsa.numTilesX = 1;
        tsa.numTilesY = explosionSprites.Length;

        for (int i = 0; i < explosionSprites.Length; i++)
        {
            tsa.SetSprite(i, explosionSprites[i]);
        }
    }
}
