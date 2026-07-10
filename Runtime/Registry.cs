using System.Collections.Generic;
using UnityEngine;

namespace Gytis0.SOManager.Runtime
{
	public class Registry : ScriptableObject
	{
		[SerializeField] private List<GameDataSO> entries = new();

		public List<GameDataSO> Entries => entries;
	}
}