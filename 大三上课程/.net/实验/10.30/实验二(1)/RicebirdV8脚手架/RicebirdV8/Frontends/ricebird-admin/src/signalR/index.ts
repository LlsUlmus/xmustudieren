import * as signalR from '@microsoft/signalr'
import { accessToken } from '@/cores/security/useCurrentUser';
const promise = new Promise<signalR.HubConnection>((resolve, reject) => {
    var builder = new signalR.HubConnectionBuilder();
    var conn = builder.withUrl("/signalr/hubs/ricebird", {
        accessTokenFactory: () => accessToken.token
    }).build();
    
    conn.start()
        .then(_ => {
            resolve(conn);
        })
        .catch(err => {
            reject(err);
        });

    conn.on("signalR-log", function ([log]) {
        console.log(log);
    });
});

export default promise;