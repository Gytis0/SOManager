using System.Collections.Generic;
using UnityEngine;

namespace SOManager.Runtime
{
	public class Registry : ScriptableObject
	{
		[SerializeField] private List<GameDataSO> entries = new();

		public List<GameDataSO> Entries => entries;
	}
}