// Run after creating the MSI; this edits the package without installing it.
// MsiLaunchApp: https://learn.microsoft.com/en-us/windows/win32/msi/msilaunchapp-controlevent
(function () {
    if (WScript.Arguments.length !== 3) {
        WScript.Echo("Usage: cscript //nologo EnableLaunchApplication.js <MSI path> <EXE file name> <checkbox text>");
        WScript.Quit(1);
    }

    var installer = new ActiveXObject("WindowsInstaller.Installer");
    var database;
    var dialog = "FinishedForm";
    var executableName = WScript.Arguments(1);
    var checkboxText = WScript.Arguments(2);
    var control = "LaunchApplicationCheckBox";
    var property = "WFL_LAUNCHAPP";
    var installCondition = 'NOT Installed AND NOT REMOVE~="ALL"';

    function quote(value) {
        return "'" + String(value).replace(/'/g, "''") + "'";
    }

    function execute(sql) {
        var view = database.OpenView(sql);
        try {
            view.Execute();
        } finally {
            view.Close();
        }
    }

    function rows(sql, fieldCount) {
        var view = database.OpenView(sql);
        var result = [];
        try {
            view.Execute();
            var record;
            while ((record = view.Fetch()) !== null) {
                var fields = [];
                for (var i = 1; i <= fieldCount; i++) {
                    fields.push(record.StringData(i));
                }
                result.push(fields);
            }
        } finally {
            view.Close();
        }
        return result;
    }

    try {
        database = installer.OpenDatabase(WScript.Arguments(0), 1);
        var files = rows("SELECT `File`, `FileName` FROM `File`", 2);
        var fileKey = null;
        for (var i = 0; i < files.length; i++) {
            var fileName = files[i][1].split("|").pop();
            if (fileName.toLowerCase() === executableName.toLowerCase()) {
                if (fileKey !== null) {
                    throw new Error("More than one " + executableName + " found in the MSI.");
                }
                fileKey = files[i][0];
            }
        }
        if (fileKey === null) {
            throw new Error(executableName + " was not found in the MSI.");
        }

        var finish = rows("SELECT `Control_`, `Ordering` FROM `ControlEvent` WHERE `Dialog_` = "
            + quote(dialog) + " AND `Event` = 'EndDialog' AND `Argument` = 'Return'", 2);
        var body = rows("SELECT `Text` FROM `Control` WHERE `Dialog_` = "
            + quote(dialog) + " AND `Control` = 'BodyText'", 1);
        if (finish.length !== 1 || body.length !== 1) {
            throw new Error("The expected installation completion dialog was not found.");
        }
        var font = body[0][0].match(/^\{[^}]+\}/);
        var caption = (font ? font[0] : "") + checkboxText;

        // Replace only this script's entries so running it again is safe.
        execute("DELETE FROM `ControlCondition` WHERE `Dialog_` = " + quote(dialog)
            + " AND `Control_` = " + quote(control));
        execute("DELETE FROM `ControlEvent` WHERE `Dialog_` = " + quote(dialog)
            + " AND `Event` = 'MsiLaunchApp' AND `Condition` = "
            + quote(property + ' = "1" AND ' + installCondition));
        execute("DELETE FROM `Control` WHERE `Dialog_` = " + quote(dialog)
            + " AND `Control` = " + quote(control));
        execute("DELETE FROM `Property` WHERE `Property` = " + quote(property));

        execute("INSERT INTO `Property` (`Property`, `Value`) VALUES ("
            + quote(property) + ", '1')");
        execute("INSERT INTO `Control` (`Dialog_`, `Control`, `Type`, `X`, `Y`, `Width`, `Height`, "
            + "`Attributes`, `Property`, `Text`, `Control_Next`) VALUES ("
            + quote(dialog) + ", " + quote(control) + ", 'CheckBox', 9, 201, 354, 12, 3, "
            + quote(property) + ", " + quote(caption) + ", 'UpdateText')");
        execute("UPDATE `Control` SET `Control_Next` = " + quote(control)
            + " WHERE `Dialog_` = " + quote(dialog) + " AND `Control` = 'BodyTextRemove'");

        var actions = ["Show", "Enable", "Hide", "Disable"];
        for (var a = 0; a < actions.length; a++) {
            var condition = a < 2 ? installCondition : 'Installed OR REMOVE~="ALL"';
            execute("INSERT INTO `ControlCondition` (`Dialog_`, `Control_`, `Action`, `Condition`) VALUES ("
                + quote(dialog) + ", " + quote(control) + ", " + quote(actions[a]) + ", " + quote(condition) + ")");
        }
        execute("INSERT INTO `ControlEvent` (`Dialog_`, `Control_`, `Event`, `Argument`, `Condition`, `Ordering`) VALUES ("
            + quote(dialog) + ", " + quote(finish[0][0]) + ", 'MsiLaunchApp', "
            + quote('[#' + fileKey + ']') + ", "
            + quote(property + ' = "1" AND ' + installCondition) + ", "
            + "0)");
        // Launch while the completion dialog is still active, then close it.
        execute("UPDATE `ControlEvent` SET `Ordering` = 1 WHERE `Dialog_` = "
            + quote(dialog) + " AND `Control_` = " + quote(finish[0][0])
            + " AND `Event` = 'EndDialog' AND `Argument` = 'Return'");
        database.Commit();
        WScript.Echo("Added the option to start " + executableName + " after installation.");
    } catch (error) {
        WScript.Echo("Could not add the launch option: " + error.message);
        WScript.Quit(1);
    }
})();
