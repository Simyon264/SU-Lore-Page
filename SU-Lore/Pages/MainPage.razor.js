let pages = []
let accountRoles = []
let dotnetHelper = null;

// roles that can skip password protection
const skipRoles = [
    0, // Admin
    1, // admin too
    2, // Moderator
]

const defaultTextSpeed = 15;

let abortController = null;

let instantText = false;
let pageCssElement = null;

function getCurrentPage() {
    return pages[pages.length - 1];
}

export function setDotnetHelper(helper) {
    dotnetHelper = helper;
}

export function initRoles(roles) {
    if (roles == null) {
        console.error("Roles are null. Perhaps user is not logged in?");
        roles = [];
    }

    console.log(`Got roles: ${roles}`);
    accountRoles = roles;
}

function CopyLinkButtonHandler() {
    let currentPage = getCurrentPage();
    let encodedPath = encodeURIComponent(currentPage.virtualPath);
    let link = `${window.location.origin}/?page=${encodedPath}`;
    navigator.clipboard.writeText(link).then(() => {
        window.showAlert("Link copied", "The link has been copied to your clipboard.");
    });
}

export async function addHandlers() {
    // HEADER:
    //  edit-button
    //  view-history-button
    //  copy-link-button
    //  raw-button
    //  stats-button
    // PASSWORD:
    //  password-submit
    //  password-skip
    // MISC:
    //  return-button

    addHandler("#edit-button", "click", EditButtonHandler);
    addHandler("#view-history-button", "click", ViewHistoryButtonHandler);
    addHandler("#copy-link-button", "click", CopyLinkButtonHandler);
    addHandler("#raw-button", "click", RawButtonHandler);
    addHandler("#stats-button", "click", StatsButtonHandler);
    addHandler("#password-submit", "click", PasswordSubmitHandler);
    addHandler("#password-skip", "click", PasswordSkipHandler);
    addHandler("#return-button", "click", ReturnButtonHandler);

    addHandler("#instant-option", "change", InstantOptionHandler);

    // General listening for anchor tags
    document.addEventListener("click", async (e) => {
        if (e.target.tagName === "A") {
            let href = e.target.getAttribute("href");
            if (!href) {
                return;
            }
            if (!href.startsWith("#")) {
                return; // We only want to handle internal links
            }
            e.preventDefault();
            if (href) {
                // remove the hash
                href = href.substring(1);
                console.log("Navigating to:", href);
                // If the link starts with /system/link, we href to whatever comes after that /
                if (href.startsWith("/system/link/")) {
                    window.location.href = href.substring(13);
                    return;
                }

                // noinspection JSIgnoredPromiseFromCall
                displayPage(await resolvePage(href));
                window.history.pushState(null, null, ` `);
            }
        } else if (e.target.tagName === "BUTTON") {
            // If we have a asp-method attribute, we want to call the respective method
            let method = e.target.getAttribute("asp-method");
            if (method) {
                console.log(`Calling method: ${method}`);
                if (method == "DeleteAccount") {
                    let result = confirm("Are you sure you want to delete your account? This will remove all files and content pages associated with your account.");
                    if (!result) {
                        return;
                    }
                }
                dotnetHelper.invokeMethodAsync(method);
            }
        }
    });

    let customCss = document.createElement("link");
    customCss.rel = "stylesheet";
    customCss.href = "/custom.css";
    document.head.appendChild(customCss);

    // We set instantText based on the "instant" cookie
    instantText = document.cookie.includes("instant=true");

    let params = new URLSearchParams(document.location.search);
    let pageInput = params.get("page");

    // we default to "/system/listing" if no page is provided
    let page = pageInput || "/system/listing";
    // noinspection JSIgnoredPromiseFromCall
    displayPage(await resolvePage(page));
}

function RawButtonHandler() {
    let currentPage = getCurrentPage();
    let url = `/page/raw?virtualPath=${currentPage.id}`;
    window.open(url, "_blank");
}

function ViewHistoryButtonHandler() {
    let currentPage = getCurrentPage();
    let url = `/history?page=${currentPage.pageGuid}`;
    window.open(url, "_blank");
}

function StatsButtonHandler() {
    let currentPage = getCurrentPage();
    let url = `/analyze?page=${currentPage.pageGuid}`;
    window.open(url, "_blank");
}

async function ReturnButtonHandler() {
    if (pages.length > 1) {
        // remove the last page
        pages = pages.slice(0, -1);
        let pageToDisplay = pages[pages.length - 1];
        // noinspection JSIgnoredPromiseFromCall
        displayPage(pageToDisplay, false);
    } else {
        // Load listing
        // noinspection JSIgnoredPromiseFromCall
        displayPage(await resolvePage("/system/listing"), false);
    }
}

function InstantOptionHandler() {
    const instantOption = document.getElementById("instant-option");
    document.cookie = `instant=${instantOption.checked}; path=/;expires=Fri, 31 Dec 9999 23:59:59 GMT`;
    instantText = instantOption.checked;
}

async function PasswordSkipHandler() {
    const currentPage = getCurrentPage();
    // noinspection JSIgnoredPromiseFromCall
    displayPage(await resolvePage(currentPage.path, "SKIP"));
}

async function PasswordSubmitHandler() {
    const passwordField = document.getElementById("password");
    const password = passwordField.value;
    const currentPage = getCurrentPage();

    // noinspection JSIgnoredPromiseFromCall
    displayPage(await resolvePage(currentPage.path, password));
}

async function displayPage(page, addToHistory = true) {
    if (page == null) {
        console.error("Could not display page. No page provided to render.");
        return;
    }

    if (pageCssElement) {
        pageCssElement.remove();
    }

    const passwordPrompt = document.getElementById("password-field");

    let passwordPage = getCurrentPage();
    if (passwordPage && passwordPage.path) { // We are currently in a password prompt
        if (page.passwordRequired) {
            // assume the password was incorrect
            window.showAlert("Incorrect password.", "The password you entered is incorrect.");
            return;
        }

        if (page.path == null) { // password was correct
            pages = pages.slice(0, -1);
        }
    }

    if (page.passwordRequired) {
        pages.push(page);

        let backbutton = document.getElementById("return-button");
        let previousPage = pages[pages.length - 2];
        if (previousPage) {
            backbutton.innerHTML = previousPage.title;
        } else {
            backbutton.innerHTML = "File listing"
        }

        backbutton.classList.remove("hidden");

        // Display password prompt
        passwordPrompt.classList.remove("hidden");
        const passwordContent = document.getElementById("content-password")
        passwordContent.classList.add("hidden");
        const guidContainer = document.getElementById("guid-container");
        guidContainer.innerHTML = page.path;

        const canSkip = accountRoles.some(role => skipRoles.includes(role));
        if (canSkip) {
            const skipButton = document.getElementById("password-skip");
            skipButton.classList.remove("hidden");
        } else {
            const skipButton = document.getElementById("password-skip");
            skipButton.classList.add("hidden");
        }

        return;
    } else {
        passwordPrompt.classList.add("hidden");
    }

    // empty the password field
    document.getElementById("password").value = "";
    setContent("");

    document.getElementById("content-password").classList.remove("hidden");

    if (addToHistory) {
        pages.push(page);
    }

    let backbutton = document.getElementById("return-button");
    let WHYISTHISNOTWORKING = hasFlag(page, "HideBackButton");
    if (WHYISTHISNOTWORKING) {
        backbutton.classList.add("hidden");
    } else {
        backbutton.classList.remove("hidden");
        let previousPage = pages[pages.length - 2];
        if (previousPage) {
            backbutton.innerHTML = previousPage.title;
        } else {
            backbutton.innerHTML = "File listing"
        }
    }

    console.debug("Displaying page:", page);

    if (page.virtualPath.startsWith("/system/")) {
        // hide the header
        document.getElementById("header").classList.add("hidden");
    } else {
        document.getElementById("header").classList.remove("hidden");
    }

    document.title = page.title;
    document.getElementById("page-title").innerHTML = page.title;

    if (hasFlag(page, "CustomCss")) {
        pageCssElement = document.createElement("link");
        pageCssElement.rel = "stylesheet";
        pageCssElement.href = `/page_custom.css?pageId=${page.id}`;
        document.head.appendChild(pageCssElement);
        console.log("Loaded custom css for page:", page.id);
    }

    // try to convert the content into html
    let url = `/page/rich_text`;
    console.log(`Fetching rich text: ${url}`);
    let response = await fetch(url, {
        method: "PATCH",
        body: JSON.stringify(page.content),
        headers: {
            "Content-Type": "application/json"
        }
    });
    let content = await response.text();

    if (hasFlag(page, "AddAccountDeleteButton")) {
        content += '<button class="button" asp-method="DeleteAccount">Delete account</button>'
        content += '<p class="block block-error">WARNING: This will delete your account and all associated data. All files and content pages will be removed. You cannot undo this.</p>'
    }

    if (abortController) {
        abortController.abort();
    }

    abortController = new AbortController();
    const { signal } = abortController;

    let currentTextSpeed = defaultTextSpeed;
    if (instantText) {
        currentTextSpeed = 0;
    }

    let loadAfterFinish = "";
    let contentString = "";

    try {
        if (hasFlag(page, "Redirect")) {
            const redirectFlag = page.flags.find(f => f.type === "Redirect");
            if (redirectFlag) {
                loadAfterFinish = redirectFlag.value;
            }
        }

        if (hasFlag(page, "EnterAnimation")) {
            const enterAnimationFlag = page.flags.find(f => f.type === "EnterAnimation");
            if (enterAnimationFlag) {
                const animation = enterAnimationFlag.value;
                switch (animation) {
                    case "FadeInAnimation":
                        // noinspection JSIgnoredPromiseFromCall
                        await ANI_FadeIn(page);
                        break;
                    case "MatrixAnimation":
                        // noinspection JSIgnoredPromiseFromCall
                        await ANI_Matrix(page);
                        break;

                    default:
                        throwError(`Unknown EnterAnimation: ${animation}, available: FadeInAnimation, MatrixAnimation`);
                        return;
                }

                if (loadAfterFinish) {
                    // noinspection JSIgnoredPromiseFromCall
                    displayPage(await resolvePage(loadAfterFinish));
                }
            }
        }

        for (let i = 0; i < content.length; i++) {
            if (signal.aborted) {
                console.warn("Aborted loading content.");
                return;
            }

            if (instantText) {
                let text = replaceForInstantRendering(content);
                setContent(text);
                contentString = text;
                break;
            }

            const c = content[i];
            contentString += c;

            if (c === '[') {
                let tag = "";
                let tagStart = i;
                while (i < content.length && content[i] !== ']' && content[i] !== '\n') {
                    tag += content[i];
                    i++;
                }
                if (content[i] === ']') {
                    tag += ']';
                }

                if (tag) {
                    i = tagStart + tag.length;
                    contentString = contentString.slice(0, -1);

                    const tagParts = tag.slice(1, -1).split('=');
                    const tagName = tagParts[0];
                    const tagValue = tagParts.length > 1 ? tagParts[1] : "";

                    switch (tagName) {
                        case "speed":
                            if (!instantText) {
                                const speed = parseInt(tagValue, 10);
                                currentTextSpeed = isNaN(speed)
                                    ? tagValue === "default"
                                        ? defaultTextSpeed
                                        : throwError(`Invalid speed: ${tagValue}`)
                                    : speed;
                            }
                            break;
                        case "delay":
                            if (!instantText) {
                                const delay = parseInt(tagValue, 10);
                                if (!isNaN(delay)) {
                                    await delayExecution(delay);
                                } else {
                                    throwError(`Invalid delay: ${tagValue}`);
                                }
                            }
                            break;

                        case "play":
                            if (!instantText) {
                                playAudio(tagValue);
                            }
                            break;
                        case "load":
                            loadAfterFinish = tagValue;
                            pages = [];
                            return;

                        default:
                            console.warn(`Unknown tag: ${tag}`);
                            contentString += c; // Restore the bracket if unknown
                            i = tagStart;
                    }
                }
            }

            setContent(contentString);

            if (hasFlag(page, "ScrollToBottom")) {
                scrollToBottom();
            }

            if (currentTextSpeed > 0) {
                playText();
                await delayExecution(currentTextSpeed);
            }
        }
    } catch (e) {
        console.error("Failed to load content:", e);
        contentString += `\n\nAn error occurred while loading the content:\n${e.message}\n${e.stack}`;
        loadAfterFinish = ""; // we wanna show the error
    } finally {
        setContent(contentString);
        abortController = null;
        if (loadAfterFinish) {
            // noinspection JSIgnoredPromiseFromCall
            displayPage(await resolvePage(loadAfterFinish));
        }
    }
}

/**
 * Checks if a page has a specific flag.
 * @param page The page to check.
 * @param flag The flag to check for.
 * @return {boolean} True if the page has the flag, false otherwise.
 */
function hasFlag(page, flag) {
    for (let i = 0; i < page.flags.length; i++) {
        if (page.flags[i].type === flag) {
            return true;
        }
    }

    return false;
}

function throwError(message) {
    throw new Error(message);
}

function setContent(content) {
    let contentElement = document.getElementById("content-text");
    contentElement.innerHTML = content;
}

function delayExecution(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

function replaceForInstantRendering(content) {
    /*
    c# method
        public static string ReplaceForInstantRendering(string content)
    {
        content = content.Replace("[speed=default]", "");
        // regex remove [speed=number]
        content = System.Text.RegularExpressions.Regex.Replace(content, @"\[speed=\d+\]", "");
        // regex remove [delay=number]
        content = System.Text.RegularExpressions.Regex.Replace(content, @"\[delay=\d+\]", "");
        // regex remove [play=audio]
        content = System.Text.RegularExpressions.Regex.Replace(content, @"\[play=[a-zA-Z0-9.]+\]", "");
        // regex remove [load=page]
        content = System.Text.RegularExpressions.Regex.Replace(content, @"\[load=[a-zA-Z0-9]+\]", "");

        return content;
    }
     */

    content = content.replace(/\[speed=default\]/g, "");
    content = content.replace(/\[speed=\d+\]/g, "");
    content = content.replace(/\[delay=\d+\]/g, "");
    content = content.replace(/\[play=[a-zA-Z0-9.]+\]/g, "");
    content = content.replace(/\[load=[a-zA-Z0-9]+\]/g, "");

    return content;
}

/**
 * Resolves the page from the input provided.
 * @param {string} input The input to resolve the page from.
 * @param {string?} password The password to use for the page.
 * @returns {object} The resolved page.
 */
async function resolvePage(input, password = null) {
    console.log("Resolving page:", input);
    const regex = /^[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}$/;
    let isValid = false;
    if (regex.test(input)) {
        isValid = true;
    }

    const intParsed = parseInt(input);
    if (!isNaN(intParsed)) {
        isValid = true;
    }

    if (input.startsWith("/")) {
        isValid = true;
    }

    if (!isValid) {
        console.error(`Provided input is not a valid page: ${input}`);
        input = "/system/notfound";
    }

    // We now do a web request to get the page
    let url = `/page?virtualPath=${input}`;
    if (password) {
        url += `&password=${password}`;
    }

    console.log(`Fetching page: ${url}`);

    let response = await fetch(url);
    console.debug(response);
    if (!response.ok) {
        // In case of a 401, we need to prompt for a password
        if (response.status === 401) {
            if (password) {
                window.showAlert("Incorrect password.", "The password you entered is incorrect.");
                return null;
            }

            return {
                passwordRequired: true,
                path: input
            };
        } else if (response.status === 404) {
            console.error(`Page not found: ${input}`);
            return {
                "id": -1,
                "version": 0,
                "title": "Not found",
                "content": "[color=red]ERROR - UNABLE TO FIND REQUESTED DOCUMENT, IF YOU BELIEVE THIS IS AN ERROR, PLEASE CONTACT ENGINEERING FOR ASSISTANCE.[/color]",
                "flags": [],
                "createdAt": "0001-01-01T00:00:00",
                "updatedAt": "0001-01-01T00:00:00",
                "createdBy": "00000000-0000-0000-0000-000000000000",
                "updatedBy": "00000000-0000-0000-0000-000000000000",
                "virtualPath": "/system/notfound",
                "pageGuid": "00000000-0000-0000-0000-000000000000"
            }
        }

        console.error(`Could not fetch page: ${input}`);
        return null;
    }

    return response.json();
}

function EditButtonHandler() {
    let currentPage = getCurrentPage();
    window.location.href = `/editor?mode=edit&id=${currentPage.pageGuid}`;
}

function addHandler(selector, event, handler) {
    document.querySelector(selector).addEventListener(event, handler);
}

window.getPages = () => pages;

////// ---- ANIMATIONS ---- //////

// Pages can select animations to play when they are displayed.
// This is done using the EnterAnimation flags.
// If a page has these, once a render is requested and the page has an animation,
// we just hand over rendering to the respective animation function.

async function ANI_FadeIn(page) {
    const CharactersPerFrame = 1;

    let contentString = page.content;
    let content = contentString.split("");
    let visibleCharacters = new Array(content.length).fill(false);

    let charactersToReveal = content.length;
    let charactersRevealed = 0;

    while (charactersRevealed < charactersToReveal) {
        let charactersThisFrame = Math.min(CharactersPerFrame, charactersToReveal - charactersRevealed);
        for (let i = 0; i < charactersThisFrame; i++) {
            let index;
            do {
                index = Math.floor(Math.random() * content.length);
            } while (visibleCharacters[index]);

            visibleCharacters[index] = true;
            charactersRevealed++;
        }

        let newContent = "";
        for (let i = 0; i < content.length; i++) {
            newContent += visibleCharacters[i] ? content[i] : " ";
        }

        setContent(newContent);
        await delayExecution(50);
    }
}

async function ANI_Matrix(page) {
    const Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+-=[]{}~";
    const Delay = 5;

    let contentString = page.content;
    let randomContent = "";
    for (let i = 0; i < contentString.length; i++) {
        randomContent += Characters[Math.floor(Math.random() * Characters.length)];
    }

    setContent(randomContent);

    let characters = randomContent.split("");
    let revealed = new Array(characters.length).fill(false);
    let revealedCount = 0;

    while (revealedCount < characters.length) {
        let index = Math.floor(Math.random() * characters.length);
        if (revealed[index]) {
            continue;
        }

        revealed[index] = true;
        characters[index] = contentString[index];
        setContent(characters.join(""));
        revealedCount++;
        await delayExecution(Delay);
    }
}