using System;

namespace Gytis0.SOManager.Runtime
{
	public interface IGameDataSO
	{
		bool IsValid();
		bool Is(Enum value);
		bool Is(GameDataSO other);
	}
}
