namespace SIL.APRE.FeatureModel
{
	/// <summary>
	/// This class represents a feature value.
	/// </summary>
	public class FeatureSymbol : IDBearerBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FeatureSymbol"/> class.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <param name="description">The description.</param>
		public FeatureSymbol(string id, string description)
			: base(id, description)
		{
		}

		public FeatureSymbol(string id)
			: this(id, id)
		{
		}

		/// <summary>
		/// Gets or sets the feature.
		/// </summary>
		/// <value>The feature.</value>
		public SymbolicFeature Feature { get; internal set; }
	}
}
