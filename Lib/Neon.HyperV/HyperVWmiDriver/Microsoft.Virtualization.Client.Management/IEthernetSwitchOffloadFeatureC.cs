using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IEthernetSwitchOffloadFeatureContract : IEthernetSwitchOffloadFeature, IEthernetSwitchFeature, IEthernetFeature, IVirtualizationManagementObject
{
	public bool DefaultQueueVrssEnabled
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public bool DefaultQueueVmmqEnabled
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public uint DefaultQueueVmmqQueuePairs
	{
		get
		{
			return 0u;
		}
		set
		{
		}
	}

	public uint DefaultQueueVrssMinQueuePairs
	{
		get
		{
			return 0u;
		}
		set
		{
		}
	}

	public uint DefaultQueueVrssQueueSchedulingMode
	{
		get
		{
			return 0u;
		}
		set
		{
		}
	}

	public bool DefaultQueueVrssExcludePrimaryProcessor
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public bool DefaultQueueVrssIndependentHostSpreading
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public bool SoftwareRscEnabled
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public abstract string InstanceId { get; }

	public abstract EthernetFeatureType FeatureType { get; }

	public abstract string Name { get; }

	public abstract string ExtensionId { get; }

	public abstract string FeatureId { get; }

	public abstract Server Server { get; }

	public abstract WmiObjectPath ManagementPath { get; }

	public event EventHandler Deleted;

	public event EventHandler CacheUpdated;

	public IVMTask BeginModifySingleFeature(IEthernetSwitchFeatureService service)
	{
		return null;
	}

	public void EndModifySingleFeature(IVMTask modifyTask)
	{
	}

	public abstract void InvalidatePropertyCache();

	public abstract void UpdatePropertyCache();

	public abstract void UpdatePropertyCache(TimeSpan threshold);

	public abstract void RegisterForInstanceModificationEvents(InstanceModificationEventStrategy strategy);

	public abstract void UnregisterForInstanceModificationEvents();

	public abstract void InvalidateAssociationCache();

	public abstract void UpdateAssociationCache();

	public abstract void UpdateAssociationCache(TimeSpan threshold);

	public abstract string GetEmbeddedInstance();

	public abstract void DiscardPendingPropertyChanges();
}
