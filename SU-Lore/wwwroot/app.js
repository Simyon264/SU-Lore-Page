window.scrollToBottom = () => {
    const element = document.getElementById("content");
    if (element) {
        element.scrollTop = element.scrollHeight;
    } else {
        console.error("Element not found");
    }
}

window.playAudio = (path) => {
    // step 1: do we already have this as an audio element?
    const audio = document.getElementById(`audio_${path}`);
    const audioContainer = document.getElementById("audio_container");
    if (audio) {
        // step 2: play it
        try {
            audio.play();
        } catch (e) {
            // ignore
        }
    } else {
        // step 3: create it
        const audio = document.createElement("audio");
        audio.id = `audio_${path}`;
        audio.src = "/resources/" + path;
        audio.autoplay = true;
        audioContainer.appendChild(audio);
    }
}

let textLastPlayed = new Date();

window.playText = () => {
    const audio = document.getElementById("text_audio");
    // if its playing, seek to the start
    if (!audio.paused) {
        // We want the text sound to play for at least 0.1 seconds
        if (new Date() - textLastPlayed < 100) {
            return;
        }
        audio.currentTime = 0;
        textLastPlayed = new Date();
    } else {
        audio.play();
    }
}

// Called on first render
window.start = (dotNetHelper) => {
    const instantOption = document.getElementById("instant-option");
    if (instantOption) {
        instantOption.addEventListener("change", function() {
            document.cookie = `instant=${instantOption.checked}; path=/`;
            try {
                dotNetHelper.invokeMethodAsync("SetInstant", instantOption.checked);
            } catch (e) {
                // Ignore, probably fine
            }
        });
    }
    
    // create event listener for any clicks on anchor tags
    document.addEventListener("click", function (e) {
        if (e.target.tagName === "A") {
            // if thee a tag has the class backlink, we want to ignore
            if (e.target.classList.contains("backlink")) {
                return;
            }

            // If we have a target="_blank" we want to open in a new tab, so ignore
            if (e.target.target === "_blank") {
                return;
            }

            e.preventDefault();

            const method = e.target.getAttribute("asp-method");
            if (method) {
                dotNetHelper.invokeMethodAsync("CallAspMethod", method);
                return;
            }
            
            // need to get only the part AFTER the #
            let id = e.target.href.split("#")[1];
            
            // If we start with /system/link/ then we redirect to what comes after the third slash
            if (id.startsWith("/system/link/")) {
                id = id.split("/").slice(3).join("/");
                window.location.href = id;
                return;
            }
            
            // We also remove the href from our current location
            window.history.pushState("", "", ``);
            
            dotNetHelper.invokeMethodAsync("LoadFromString", id, true);
        } else if (e.target.tagName === "BUTTON") {
            const deleteAtt = e.target.getAttribute("file-manager-delete")
            if (deleteAtt) {
                e.preventDefault()
                dotNetHelper.invokeMethodAsync("DeleteFile", deleteAtt);
                return;
            }
            
            // If the button has a asp-method attribute, we want to use that
            const method = e.target.getAttribute("asp-method");
            if (method) {
                e.preventDefault();
                // if we have a is-password attribute, we want to do the reverse of showPasswordPrompt
                const isPassword = e.target.getAttribute("is-password");
                if (isPassword) {
                    const passwordPrompt = document.getElementById("password-field");
                    passwordPrompt.classList.add("hidden");
                    const passwordContent = document.getElementById("content-password")
                    passwordContent.classList.remove("hidden");
                }
                
                switch (method) {
                    case "SaveNewColor":
                        const newColor = document.getElementById("new-color").value;
                        const newName = document.getElementById("new-name").value;
                        dotNetHelper.invokeMethodAsync("SaveNewColor", newColor, newName);
                        break;
                    case "SaveExisting":
                        // Get the ID tag of the clicked button
                        const id = e.target.id;
                        const color = document.getElementById(`color-${id}`).value;
                        const name = document.getElementById(`name-${id}`).value;
                        const idButInt = parseInt(id);
                        dotNetHelper.invokeMethodAsync("SaveExisting", idButInt, color, name);
                        break;
                    case "Default":
                        dotNetHelper.invokeMethodAsync("CallAspMethod", method);
                        break;
                        
                }
            }
            const toggleDiff = e.target.getAttribute("asp-toggle-diff");
            if (toggleDiff) {
                e.preventDefault();
                dotNetHelper.invokeMethodAsync("ToggleDiff", toggleDiff);
            }
        }
    });

    const fileUploadForm = document.getElementById("uploadForm");
    if (fileUploadForm) {
        fileUploadForm.addEventListener("submit", function(event) {
            event.preventDefault();
            const fileInput = document.getElementById("fileInput");
            if (fileInput.files.length < 1) {
                window.showAlert("Error", "No file selected");
                return;
            }
            const file = fileInput.files[0];

            const formData = new FormData();
            formData.append("file", file);

            const uploadProgress = document.getElementById("uploadProgress");
            const processingMessage = document.getElementById("processingMessage");

            const xhr = new XMLHttpRequest();
            xhr.upload.onprogress = function(e) {
                if (e.lengthComputable) {
                    const percentComplete = (e.loaded / e.total) * 100;
                    uploadProgress.style.width = percentComplete + '%';
                    if (percentComplete === 100) {
                        processingMessage.style.display = 'block';
                    }
                }
            };
            xhr.onreadystatechange = function() {
                if (xhr.readyState === XMLHttpRequest.DONE) {
                    processingMessage.style.display = 'none';
                    if (xhr.status === 200) {
                        window.location.reload();
                    } else {
                        window.showAlert("Error", `Failed to upload file: ${xhr.responseText}`);
                    }
                }
            };
            xhr.open('PUT', '/resources/', true);
            xhr.send(formData);
        });
    }
}

window.setAlertOnLeave = () => {
    window.onbeforeunload = function() {
        return true;
    };
}

window.clearAlertOnLeave = () => {
    window.onbeforeunload = null;
}

window.showAlert = (header, message) => {
    const alert = document.getElementById("alert-box");
    document.getElementById("alert-header").textContent = header;
    document.getElementById("alert-text").textContent = message;
    document.getElementById("parent").classList.add("hidden");
    
    alert.classList.add("alert-visible");
    // Add event listener to close the alert
    document.getElementById("close-button").addEventListener("click", () => {
        alert.classList.remove("alert-visible");
        // also remove the event listener
        document.getElementById("close-button").removeEventListener("click", () => {});
        document.getElementById("parent").classList.remove("hidden");
    });
}

window.getContentFromDomId = (id) => {
    const element = document.getElementById(id);
    if (element) {
        return element.innerHTML;
    }
    return "";
}

window.getValueFromInputElement = (id) => {
    const element = document.getElementById(id);
    if (element) {
        return element.value;
    }
    return "";
}

window.getCheckBoxValue = (id) => { 
    const element = document.getElementById(id);
    if (element) {
        return element.checked.toString();
    }
    return false.toString();
}

window.copyToClipboard = (text) => {
    navigator.clipboard.writeText(text).then(() => {
        window.showAlert("Success", "Copied to clipboard");
    }).catch(() => {
        window.showAlert("Error", "Could not copy to clipboard");
    })
}

window.showPasswordPrompt = (pageGuid) => {
    const passwordPrompt = document.getElementById("password-field");
    passwordPrompt.classList.remove("hidden");
    const passwordContent = document.getElementById("content-password")
    passwordContent.classList.add("hidden");
    const guidContainer = document.getElementById("guid-container");
    guidContainer.innerHTML = pageGuid;
}

function videoError(id) {
    const element = document.getElementById(id);
    console.debug("Loading fallback image for video");
    element.src = "/resources/missing.mp4";

    element.loop = true;
    element.load();
    element.play();
}