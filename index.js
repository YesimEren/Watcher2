const express = require('express');
const axios = require('axios');
const path = require('path');
const fs = require('fs');
const cors = require('cors');
const app = express();
const port = 3000;

// Express middleware
app.use(express.json());
app.use(cors());

let isVirtualMachineRunning = true;

function updateStatus(isRunning) {
    axios.get('http://localhost:5145/api/watcher/memory')
        .then(response => {
            const isVMRunning = response.data.isRunning; //.IsRunning
            updateVMStatus(isVMRunning);
        })
        .catch(error => {
            console.error(error);
        });
    //Docker, Let's Encrypt ve Memory durumlarına benzer güncellemleri bueraya yapoılacak.
}

function logToFile(logMessage) {
    const logFile = path.join(__dirname, 'log.txt');
    fs.appendFileSync(logFile, `${new Date()} - ${logMessage}\n`);
}
//function updateMemoryStatus() {
//    axios.get('http://localhost:5145/api/watcher/memory')
//        .then(response => {
//            const { total, used } = response.data;
//            const memoryStatusElement = document.getElementById('memoryStatus');
//            const memoryUsagePercentage = (used / total) * 100;

//            memoryStatusElement.style.backgroundColor = memoryUsagePercentage > 80 ? 'red' : 'green';
//        })
//        .catch(error => {
//            console.error(error);
//        });
//}
setInterval(() => {
    updateStatus();
    if (isVirtualMachineRunning) {
        logToFile('Virtual Machine is running.');
    } else {
        logToFile('Virtual Machine is stopped.');
    }
}, 10000); // Her 10 saniyede bir kontrol et

app.get('/status', (req, res) => {
    res.json({ isRunning: isVirtualMachineRunning });
});

app.get('/', (req, res) => {
    res.sendFile(path.join(__dirname, 'index.html'));
});

app.listen(port, () => {
    console.log(`Frontend app listening at http://localhost:${port}`);
});



