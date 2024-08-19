import * as signalR from '@microsoft/signalr';

const hubConnection = new signalR.HubConnectionBuilder()
  .withUrl(process.env.REACT_APP_SIGNAL_APP_API_URL!, { withCredentials: false })
  .withAutomaticReconnect()
  .configureLogging(signalR.LogLevel.Information)
  .build();

  export const startConnection = async () => {
    if (hubConnection.state === signalR.HubConnectionState.Disconnected) {
      try {
        await hubConnection.start();
        console.log('SignalR Connected');
      } catch (err) {
        console.error('Error while establishing SignalR connection: ', err);
      }
    } else {
      console.log('SignalR connection is already started or in progress.');
    }
  };

export const subscribeToNewRides = (callback: (ride: any) => void) => {
  hubConnection.on('NewRide', (ride) => {
    console.log('Primljen događaj NewRide:', ride);
    callback(ride);
  });
};

export const subscribeToRideConfirmation = (callback: (ride: any) => void) => {
  hubConnection.on('RideConfirmed', (ride) => {
    console.log('Ride confirmed:', ride);
    callback(ride);
  });
};

export const subscribeToRideTimeUpdate = (callback: (timeUpdate: number) => void) => {
  hubConnection.on('UpdateRideTime', (timeUpdate) => {
    console.log('Ride time updated:', timeUpdate);
    callback(timeUpdate);
  });
};

export const subscribeToRideFinished = (callback: (rideId: string) => void) => {
  hubConnection.on('RideFinished', (rideId: string) => {
    console.log('Ride finished:', rideId);
    callback(rideId);
  });
};

export const unsubscribeFromAll = () => {
  hubConnection.off('NewRide');
  hubConnection.off('RideConfirmed');
  hubConnection.off('UpdateRideTime');
  hubConnection.off('RideFinished');
};

export default hubConnection;
