function ClosePopupWindow() {
    window.open('', '_self', '');
    window.close();
}

//Limit length of input in a textbox to a fixed number of characters
function limitTextLength(limitField, limitNum) {
    if (limitField.value.length > limitNum) {
        limitField.value = limitField.value.substring(0, limitNum);
    }
}

//Limit length of input in a textbox to 0-9 only
function limitTextToNumeric(limitField, character) {
    if (character < 48 || character > 57) {
        event.returnValue = false;
        event.cancel = true;
    }
}

function popWindowFullScreen() {
    if (this.window.top.location !== this.window.self.location) {
        this.window.top.location = this.window.self.location;
    }
    this.window.moveTo(0, 0);
    this.window.resizeTo(this.screen.width, this.screen.height);
    if (this.document.getElementById('DataGrid1').parentNode) {
        var gridDiv = this.document.getElementById('DataGrid1').parentNode;
        gridDiv.style.height = '100%';
        gridDiv.style.width = '100%';
    }
}


