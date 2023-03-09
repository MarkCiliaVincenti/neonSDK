using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IEthernetPortAllocationSettingDataContract : IEthernetPortAllocationSettingData, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
	public WmiObjectPath[] HostResources
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public WmiObjectPath HostResource
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public string Address
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public string TestReplicaPoolId
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public string TestReplicaSwitchName
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public IEnumerable<IEthernetSwitchPortFeature> Features => null;

	public abstract string FriendlyName { get; set; }

	public abstract string DeviceTypeName { get; }

	public abstract string DeviceId { get; }

	public abstract string PoolId { get; set; }

	public abstract Guid VMBusChannelInstanceGuid { get; set; }

	public abstract VMDeviceSettingType VMDeviceSettingType { get; }

	public abstract IVMDevice VirtualDevice { get; }

	public abstract IVMComputerSystemSetting VirtualComputerSystemSetting { get; }

	public abstract Server Server { get; }

	public abstract WmiObjectPath ManagementPath { get; }

	public event EventHandler Deleted;

	public event EventHandler CacheUpdated;

	public abstract IVMTask BeginPut();

	public abstract void EndPut(IVMTask putTask);

	public abstract void Put();

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
[WmiName("Msvm_EthernetPortAllocationSettingData")]
internal interface IEthernetPortAllocationSettingData : IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
	WmiObjectPath[] HostResources { get; set; }

	WmiObjectPath HostResource { get; set; }

	string Address { get; set; }

	string TestReplicaPoolId { get; set; }

	string TestReplicaSwitchName { get; set; }

	IEnumerable<IEthernetSwitchPortFeature> Features { get; }
}
