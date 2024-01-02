const express = require('express');
const cors = require('cors');
const { exec } = require('child_process');
const axios = require('axios');
const path = require('path');
const fs = require('fs');
const app = express();
const port = 3000;

// Express middleware
app.use(express.json());
app.use(cors());
//app.use(corscors({
//    origin: 'http://localhost:3000',  // İzin verilen kaynak (frontend uygulamasının adresi)
//    methods: 'GET,HEAD,PUT,PATCH,POST,DELETE',
//    credentials: true,
//}));

let isVirtualMachineRunning = true;

// Memory API endpoint'i
app.get('/api/memory', (req, res) => {
    const command = 'VBoxManage guestcontrol yesubuntu exec --image /usr/bin/free --username ysert --password yesimsert --wait-stdout -- -m';

    exec(command, (error, stdout, stderr) => {
        if (error) {
            console.error(`Error: ${error.message}`);
            return res.status(500).json({ error: 'Internal Server Error' });
        }

        const usedMemory = parseUsedMemory(stdout);
        const totalMemory = parseTotalMemory(stdout);
        const usagePercentage = (usedMemory / totalMemory) * 100;

        // E?ik de?eri kontrolü yap
        const status = usagePercentage >= 80 ? 'Red' : 'Green';

        res.json({
            UsedMemory: usedMemory,
            TotalMemory: totalMemory,
            UsagePercentage: usagePercentage.toFixed(2),
            Status: status
        });
    });
});

// Sunucuyu başlat
app.listen(port, () => {
    console.log(`Server is running on port ${port}`);
});

function parseUsedMemory(output) {
    const lines = output.split('\n');
    const usedMemoryLine = lines[1];
    const values = usedMemoryLine.split(/\s+/);
    return parseFloat(values[2]);
}

function parseTotalMemory(output) {
    const lines = output.split('\n');
    const totalMemoryLine = lines[1];
    const values = totalMemoryLine.split(/\s+/);
    return parseFloat(values[1]);
}

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
