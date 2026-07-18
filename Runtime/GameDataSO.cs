using System;
using System.Runtime.CompilerServices;
using UnityEngine;
[assembly: InternalsVisibleTo("Gytis0.SOManager.Editor")]

#pragma warning disable CS0809

namespace Gytis0.SOManager.Runtime
{
	public abstract class GameDataSO : ScriptableObject, IGameDataSO
	{
		[SerializeField, HideInInspector] private string guid;
		[SerializeField] private string enumName;
		[SerializeField, HideInInspector] private int enumId;
		[SerializeField, HideInInspector] private bool isDeleted;
		[SerializeField] private string displayName;
		[SerializeField] private Sprite icon;

		public string Guid => guid;
		public string EnumName => enumName;
		public int EnumId => enumId;
		public bool IsDeleted => isDeleted;

		public string Name => displayName;
		public Sprite Icon => icon;

		/// <summary>
		/// Checks whether this asset is valid.
		/// </summary>
		/// <remarks>
		/// By default it always returns true. If you want to use the validation, override this method.
		/// </remarks>
		/// <returns><see langword="true"/> if valid. Otherwise, <see langword="false"/></returns>
		public virtual bool IsValid()
		{
			return true;
		}

		public virtual bool Is(Enum value)
		{
			return EnumId == Convert.ToInt32(value);
		}

		public virtual bool Is(GameDataSO other)
		{
			return other != null && EnumId == other.EnumId && GetType() == other.GetType();
		}

		public override string ToString() { return Name; }

		public bool Is<T>() where T : GameDataSO
		{
			return this is T;
		}

		#region Internal

		internal void GenerateGuid()
		{
			guid = System.Guid.NewGuid().ToString("N");
		}

		internal void SetEnumName(string value)
		{
			enumName = value;
		}

		internal void SetEnumId(int value)
		{
			enumId = value;
		}

		internal void SetSoftDeleted(bool value)
		{
			isDeleted = value;
		}

		internal void SetName(string value)
		{
			displayName = value;
		}

		internal void SetIcon(Sprite icon)
		{
			this.icon = icon;
		}

		internal string GetIdentifyingName()
		{
#if UNITY_6000_5_OR_NEWER
			return !string.IsNullOrWhiteSpace(Name) ? Name :
				!string.IsNullOrWhiteSpace(EnumName) ? EnumName :
				!string.IsNullOrWhiteSpace(Guid) ? Guid :
				GetEntityId().ToString();
#elif UNITY_6000_1_OR_NEWER
		return !string.IsNullOrWhiteSpace(Name) ? Name :
			!string.IsNullOrWhiteSpace(EnumName) ? EnumName :
			!string.IsNullOrWhiteSpace(Guid) ? Guid :
			GetInstanceID().ToString();
#endif
		}

		#endregion
	}

	public abstract class GameDataSO<TEnum> : GameDataSO
		where TEnum : Enum
	{
		private TEnum enumValue;
		[HideInInspector] public TEnum Enum => enumValue;

		protected virtual void OnEnable()
		{
			enumValue = (TEnum)System.Enum.ToObject(typeof(TEnum), EnumId);
		}

		public bool Is(TEnum value)
		{
			return EnumId == Convert.ToInt32(value);
		}

		public bool Is(GameDataSO<TEnum> other)
		{
			return other != null && EnumId == other.EnumId;
		}

		[Obsolete("This overload is unsafe. Prefer using the generic one, as it provides compile-time safety.", false)]
		public override bool Is(GameDataSO other)
		{
			return base.Is(other);
		}

		[Obsolete("This overload is unsafe. Prefer using the generic one, as it provides compile-time safety.", false)]
		public override bool Is(Enum value)
		{
			if (typeof(TEnum) != value.GetType()) throw new ArgumentException(string.Format("Expected enum of type '{0}', but got '{1}'.", typeof(TEnum).Name, value.GetType().Name), nameof(value));
			return base.Is(value);
		}
	}
}