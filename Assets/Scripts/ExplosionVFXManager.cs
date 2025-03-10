using UnityEngine;

public class ExplosionVFXManager : MonoBehaviour
{
    [Header("Explosion Sprites")]
    [SerializeField] private Sprite[] _explosionSprites;

    public Sprite[] GetExplosionSpritesForBlock(int blockSpriteIndex)
    {
        int startIndex = blockSpriteIndex * 3;
        if (_explosionSprites == null || _explosionSprites.Length < startIndex + 3)
        {
            return null;
        }
        Sprite[] set = new Sprite[3];
        for (int i = 0; i < 3; i++)
        {
            set[i] = _explosionSprites[startIndex + i];
        }
        return set;
    }
}
