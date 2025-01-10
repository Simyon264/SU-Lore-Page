const connection = new signalR.HubConnectionBuilder()
    .withUrl("/api/usercount")
    .build();

connection.on("UpdateUserCount", (count) => {
    const userCountElement = document.getElementById('usercount');
    userCountElement.innerText = count;
    console.log("User count updated: " + count);
});

connection.start()
    .catch(err => console.error(err))
    .then(() => getUserCount());

setInterval(getUserCount, 10000);

function getUserCount() {
    connection.invoke("GetUserCount")
        .catch(err => console.error(err));
}

setTimeout(getUserCount, 1000);