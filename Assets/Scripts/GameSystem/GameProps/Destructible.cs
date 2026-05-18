using GameSystem.GameProps.Item;
using GameSystem.GameScene;
using GameSystem.Manager;
using UnityEngine;

namespace GameSystem.GameProps
{
    public class Destructible : BaseObject
    {
        public PropsStatus CreateItem()
        {
            if (PropsManager.Instance.CreateProps(out var propsConfig) && GameModeSelect.CurrentModeType == GameModeType.Offline)
            {
                // 创建道具
                var item = Instantiate(propsConfig.propsObj, transform.position, Quaternion.identity);
                item.transform.SetParent(PropsManager.Instance.transform);
                var ps = item.GetComponent<PropsStatus>();
                if (ps == null)
                {
                    Debug.LogError("PropsStatus not found");
                }
                ps.propsConfig = propsConfig;
                return ps;
            }
            return null;
        }
    }
}