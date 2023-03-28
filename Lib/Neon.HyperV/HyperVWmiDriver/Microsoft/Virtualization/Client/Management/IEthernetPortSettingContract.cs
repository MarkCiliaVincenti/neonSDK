using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IEthernetPortSettingContract : IEthernetPortSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
	public bool IsNetworkAddressStatic
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public string NetworkAddress
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public IEthernetPort EthernetDevice => null;

	public bool ClusterMonitored
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public IVMBootEntry BootEntry => null;

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

	public IEthernetConnectionAllocationRequest GetConnectionConfiguration()
	{
		return null;
	}

	public IGuestNetworkAdapterConfiguration GetGuestNetworkAdapterConfiguration()
	{
		return null;
	}

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

	public abstract IVMTask BeginDelete();

	public abstract void EndDelete(IVMTask deleteTask);

	public abstract void Delete();
}
