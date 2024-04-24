# WIP - HGS Reinforced Learning Agents

## Startup

1. Create an class for your agent extending `Agent`, and implement the follow methods:

```csharp
protected override void ExecuteAction(int action)
{
    switch (action)
    {
        case 0: _dir = Vector2.left; break;
        case 1: _dir = Vector2.up; break;
        case 2: _dir = Vector2.right; break;
        case 3: _dir = Vector2.down; break;
        case 4: _dir = Vector2.left + Vector2.up; break;
        case 5: _dir = Vector2.right + Vector2.up; break;
        case 6: _dir = Vector2.left + Vector2.down; break;
        case 7: _dir = Vector2.right + Vector2.down; break;
    }
}

protected override float GetReward()
{
    var reward = 1f - _endDistance / 3f;
    if (_endDistance <= 1.5f) return reward * 2f;
    if (_endDistance > _initialDistance) return -1f;
    return reward;
}

protected override float[] GetState()
{
    var direction = (transform.position - target.position).normalized;
    return new float[] { direction.x, direction.y };
}
```

2. Create a Brain ScriptableObject and attach in you agent
<img width="796" alt="image" src="https://github.com/homy-game-studio/hgs-unity-rl-agents/assets/6765023/cfcaa2fd-3864-43ab-8f6b-1b033aede674">

3. Ajdust params and train


## Progress

1. Traning - First epochs
   
https://github.com/homy-game-studio/hgs-unity-rl-agents/assets/6765023/17a177ee-0919-4f28-af2e-983ca22dc51b

3. Training - Last epochs
   
https://github.com/homy-game-studio/hgs-unity-rl-agents/assets/6765023/7958864c-b93b-49ca-8b4a-a79826420488

4. Final Result

https://github.com/homy-game-studio/hgs-unity-rl-agents/assets/6765023/e58fcc18-646a-43be-bba1-d9a88b4bb850
