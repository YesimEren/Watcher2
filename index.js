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

function updateStatus() {
    axios.get('http://localhost:5145/api/virtualmachine/status')
        .then(response => {
            const isVirtualMachineRunning = response.data.isRunning; 
            updateVMStatus(isVMRunning);
        })
        .catch(error => {
            console.error(error);
        });

    //Docker, Let's Encrypt ve Memory durumlarına benzer güncellemleri bueraya yapoılacak.
}

setInterval(() => {
    updateStatus();
}, 5000);

app.get('/status', (req, res) => {
    res.json({ IsRunning: isVirtualMachineRunning });
});

app.get('/', (req, res) => {
    res.sendFile(path.join(__dirname, 'index.html'));
});

app.listen(port, () => {
    console.log(`Frontend app listening at http://localhost:${port}`);
});

// Logları dosyaya kaydet
function logToFile(logMessage) {
    const logFile = path.join(__dirname, 'log.txt');
    fs.appendFileSync(logFile, `${new Date()} - ${logMessage}\n`);
}

// Virtual Machine durumuna göre log kayıtları
setInterval(() => {
    updateStatus();
    if (isVirtualMachineRunning) {
        logToFile('Virtual Machine is running.');
    } else {
        logToFile('Virtual Machine is stopped.');
    }
}, 10000); // Her 10 saniyede bir kontrol et

