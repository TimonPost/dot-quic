use crate::{
    ffi::{
        bindings::callbacks,
    },
    proto,
    proto_impl::{
        connection::{
            ConnectionEvent,
            ConnectionInner,
        },
    },
};



use quinn_proto::Transmit;

use std::{
    collections::{
        HashMap,
    },
    sync::{
        atomic::{
            AtomicU8,
            Ordering,
        },
        mpsc,
    },
};

static ENDPOINT_ID: AtomicU8 = AtomicU8::new(0);

#[derive(Debug)]
pub enum EndpointEvent {
    Proto(proto::EndpointEvent),
    Transmit(proto::Transmit),
}

pub struct EndpointInner {
    pub(crate) inner: proto::Endpoint,
    connections: HashMap<proto::ConnectionHandle, mpsc::Sender<ConnectionEvent>>,
    endpoint_events_rx: mpsc::Receiver<(proto::ConnectionHandle, EndpointEvent)>,
    endpoint_events_tx: mpsc::Sender<(proto::ConnectionHandle, EndpointEvent)>,
    pub id: u8,
}

impl EndpointInner {
    pub fn new(endpoint: proto::Endpoint) -> Self {
        let (tx, rx) = mpsc::channel();

        let id = ENDPOINT_ID.load(Ordering::Relaxed).wrapping_add(1);

        EndpointInner {
            inner: endpoint,
            connections: HashMap::new(),
            endpoint_events_tx: tx,
            endpoint_events_rx: rx,
            id,
        }
    }

    pub fn poll(&mut self) {
        if let Some(transmit) = self.inner.poll_transmit() {
            self.notify_transmit(transmit);
        }

        self.handle_connection_events();
    }

    pub fn notify_transmit(&mut self, transmit: Transmit) {
        callbacks::on_transmit(self.id, transmit);
    }

    pub fn add_connection(
        &mut self,
        handle: proto::ConnectionHandle,
        connection: proto::Connection,
    ) -> ConnectionInner {
        let (send, recv) = mpsc::channel();
        let _ = self.connections.insert(handle, send);

        ConnectionInner {
            inner: connection,
            connected: false,
            connection_events: recv,
            endpoint_events: self.endpoint_events_tx.clone(),
            connection_handle: handle,
        }
    }

    pub fn forward_event_to_connection(
        &mut self,
        handle: proto::ConnectionHandle,
        event: proto::ConnectionEvent,
    ) {
        let _ = self
            .connections
            .get_mut(&handle)
            .unwrap()
            .send(ConnectionEvent::Proto(event));
    }

    pub fn handle_connection_events(&mut self) {
        while let Ok((handle, event)) = self.endpoint_events_rx.try_recv() {
            match event {
                EndpointEvent::Proto(proto) => {
                    if proto.is_drained() {
                        self.connections.remove(&handle);
                        if self.connections.is_empty() {
                            //self.idle.notify_waiters();
                        }
                    }

                    if let Some(event) = self.inner.handle_event(handle, proto) {
                        // Ignoring errors from dropped connections that haven't yet been cleaned up
                        let _ = self
                            .connections
                            .get_mut(&handle)
                            .unwrap()
                            .send(ConnectionEvent::Proto(event));
                    }
                }
                EndpointEvent::Transmit(transmit) => {
                    self.notify_transmit(transmit);
                }
            }
        }
    }
}
