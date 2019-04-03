//
//  ProjectDataSource.swift
//  TogglDesktop
//
//  Created by Nghia Tran on 3/25/19.
//  Copyright © 2019 Alari. All rights reserved.
//

import Foundation

class ProjectHeaderItem {

    let name: String
    let item: AutocompleteItem

    init(item: AutocompleteItem) {
        self.item = item
        self.name = item.clientLabel
    }
}

class ProjectContentItem {

    let name: String
    let colorHex: String
    let item: AutocompleteItem

    init(item: AutocompleteItem) {
        self.item = item
        self.name = item.projectLabel
        self.colorHex = item.projectColor

        if item.projectID == 0 {
            print("== 0 \(self.name)")
        }
    }
}

final class ProjectDataSource: AutoCompleteViewDataSource {

    private struct Constants {

        static let HeaderCell = NSUserInterfaceItemIdentifier("ProjectHeaderCellView")
        static let HeaderNibName = NSNib.Name("ProjectHeaderCellView")
        static let ProjectCell = NSUserInterfaceItemIdentifier("ProjectContentCellView")
        static let ProjectNibName = NSNib.Name("ProjectContentCellView")
    }

    // MARK: Variables

    override func registerCustomeCells() {
        tableView.register(NSNib(nibNamed: Constants.HeaderNibName, bundle: nil),
                            forIdentifier: Constants.HeaderCell)
        tableView.register(NSNib(nibNamed: Constants.ProjectNibName, bundle: nil),
                            forIdentifier: Constants.ProjectCell)
    }

    override func filter(with text: String) {

        // show all
        if text.isEmpty {
            render(with: ProjectStorage.shared.items)
            return
        }

        // Filter
        let filterItems = ProjectStorage.shared.filter(with: text)
        render(with: filterItems)
    }

    // MARK: Public

    override func tableView(_ tableView: NSTableView, viewFor tableColumn: NSTableColumn?, row: Int) -> NSView? {
        let item = items[row]
        switch item {
        case let header as ProjectHeaderItem:
            let view = tableView.makeView(withIdentifier: Constants.HeaderCell, owner: self) as! ProjectHeaderCellView
            view.render(header)
            return view
        case let project as ProjectContentItem:
            let view = tableView.makeView(withIdentifier: Constants.ProjectCell, owner: self) as! ProjectContentCellView
            view.render(project)
            return view
        default:
            return nil
        }
    }

    override func tableView(_ tableView: NSTableView, heightOfRow row: Int) -> CGFloat {
        let item = items[row]
        switch item {
        case is ProjectHeaderItem:
            return ProjectHeaderCellView.cellHeight
        case is ProjectContentItem:
            return ProjectContentCellView.cellHeight
        default:
            return 0
        }
    }

    override func tableView(_ tableView: NSTableView, shouldSelectRow row: Int) -> Bool {
        let item = items[row]
        if item is ProjectContentItem {
            return true
        }
        return false
    }
}
